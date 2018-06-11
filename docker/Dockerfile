#	Multi-stage docker build file
#	Use a Microsoft image with .NET core runtime (https://hub.docker.com/r/microsoft/dotnet/tags/)
FROM microsoft/dotnet:2.1-sdk AS build

WORKDIR /src

#	Copy source code into the source folder
COPY . .

#	Publish the app into the app folder
RUN dotnet publish . -c release -o app

###########################################################
#	Final container image
#	Use a Microsoft image with .NET core runtime (https://hub.docker.com/r/microsoft/dotnet/tags/)
FROM microsoft/dotnet:2.1-runtime AS final

#	Set the working directory to /work
WORKDIR /work

#	Copy package
COPY --from=build /src/app .

#	Define environment variables
ENV ACCOUNT_ENDPOINT ""
ENV ACCOUNT_KEY ""
ENV TARGET_URL ""

#	Run console app
CMD ["dotnet", "CosmosTargetConsole.dll"]
