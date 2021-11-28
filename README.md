# TrueLayer Coding Challenge

This web API describes Pokemon characters in a uniform JSON response format. It sources its information primarily from 
https://pokeapi.co/ but can optionally decorate ("translate") responses as described below.

## What it does

There are two endpoints:
* `GET /pokemon/{name}`, where "name" is the name of a Pokemon character. The response looks like this:

      {
        "name": "mewtwo",
        "description": "It was created by a scientist after years of horrific gene splicing and DNA engineering experiments.",
        "habitat": "rare",
        "isLegendary": true
      }

* `GET /pokemon/translated/{name}`, where "name" is the name of a Pokemon character. The response is the same as for
the previous endpoint, except that the description might be "translated":

      {
        "name": "mewtwo",
        "description": "Created by a scientist after years of horrific gene splicing and dna engineering experiments, it was.",
        "habitat": "rare",
        "isLegendary": true
      }

    The translation will either be a Yoda immitation, as above, or a faux-Shakespearean rendition.

## Where the data comes from

The Pokemon data comes from https://pokeapi.co/. The translations come from https://api.funtranslations.com/.

## How to run it

Make sure you have Docker installed. For more information see https://docs.docker.com/get-docker/.

If you want to run a **debug** build which includes a Swagger UI for testing, follow these steps:
1. Open a terminal in the [src/](src/) directory.
1. Build and run the container:

       docker build --target debug -t truelayer .
       docker run -it -p 5000:5000 truelayer

    If you want the container to run in the background rather than in your teminal, substitute `-d` for `-it`. Note
also that if you run the container from Git bash on Windows, you might need to prefix the second command with `winpty`.
1. Open http://localhost:5000/swagger in your web browser to use the Swagger UI, or simply hit the API endpoints from a
tool of your choice.

If you want to run a **production** build against a minimal ASP.NET Core image, follow these steps:
1. Open a terminal in the [src/](src/) directory.
1. Build and run the container (in the background):

       docker build -t truelayer .
       docker run -d -p 5000:5000 truelayer

## How to develop it

The code is written in C#10 for .NET 6.0 and ASP.NET Core 6.0. Visual Studio 2022 has support for these technologies.

## How to test it

The solution comes with both integration and unit tests. The integration tests rely on
[WireMock.Net](https://github.com/WireMock-Net/WireMock.Net) to replay web requests which were recorded in the manner
described on their [Wiki](https://github.com/WireMock-Net/WireMock.Net/wiki/Settings#proxyandrecordsettings).

The unit tests focus on unusual scenarios which are not already covered by the integration tests.

All tests will run automatically as part of the debug build (see above).

## Code structure

As the requirements are straightforward a single project suffices. However it would be possible to switch out
Infrastructure and Presentation concerns easily, in the spirit of Clean Architecture. Only types which are needed in an
outer "layer" have been marked as `public`.

The reason for the command pattern (`IRequestCommand`) is that RESTful API calls are daisy chained when consuming
https://pokeapi.co/. It is not necessary to expose the exact URLs, however the command pattern allows the Application
code to remain in the driving seat.

# Productionising

Before deploying this code to a production environment, this following considerations should be taken into account:

* Caching: It should not be necessary to hit third party APIs repeatedly with the same requests. A cache, e.g. Redis,
could stand in the middle.
* Resiliency: HTTP requests could be retried in the event of transient (e.g. network-related) issues. It would be
trivial to implement this via a decorator around the `IRequestCommand` interface.
* Security: Consider running the application as a non-privileged user inside the container. Currently the application
runs as `root`.
* Licensing: No licence is stipulated at present.
