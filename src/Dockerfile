# Stage 1: Restore NuGet packages, which is slow and depends only on the SLN and CSPROJs.

FROM mcr.microsoft.com/dotnet/sdk:6.0 as restore

WORKDIR /src
COPY Morphologue.Challenges.TrueLayer.sln ./
COPY Morphologue.Challenges.TrueLayer/Morphologue.Challenges.TrueLayer.csproj Morphologue.Challenges.TrueLayer/
COPY Morphologue.Challenges.TrueLayer.Tests/Morphologue.Challenges.TrueLayer.Tests.csproj Morphologue.Challenges.TrueLayer.Tests/

RUN dotnet restore


# Stage 2: Build in debug mode and test then run. One might target this stage during development.

FROM mcr.microsoft.com/dotnet/sdk:6.0 as debug

WORKDIR /src
COPY . ./
COPY --from=restore /root/.nuget /root/.nuget/
COPY --from=restore /src/Morphologue.Challenges.TrueLayer/obj Morphologue.Challenges.TrueLayer/obj/
COPY --from=restore /src/Morphologue.Challenges.TrueLayer.Tests/obj Morphologue.Challenges.TrueLayer.Tests/obj/
RUN dotnet build -c Debug --no-restore
RUN cd Morphologue.Challenges.TrueLayer.Tests && dotnet test --no-build
WORKDIR Morphologue.Challenges.TrueLayer
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "run", "--no-build"]


# Stage 3: Build in release mode then publish.

FROM mcr.microsoft.com/dotnet/sdk:6.0 as publish

WORKDIR /src/Morphologue.Challenges.TrueLayer
COPY Morphologue.Challenges.TrueLayer ./
COPY --from=restore /root/.nuget /root/.nuget/
COPY --from=restore /src/Morphologue.Challenges.TrueLayer/obj obj/
RUN dotnet publish --no-restore -c Release -o /app


# Stage 4: Run in an optimised production image.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as release

WORKDIR /app
COPY --from=publish /app ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "Morphologue.Challenges.TrueLayer.dll"]
