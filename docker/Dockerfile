# Use a Microsoft image with .NET core runtime (https://hub.docker.com/r/microsoft/dotnet/tags/)
FROM microsoft/dotnet:2.1-runtime

# Set the working directory to /work
WORKDIR /work

# Copy DLL
ADD temp/* /work/

# Define environment variables
ENV ACCOUNT_ENDPOINT ""
ENV ACCOUNT_KEY ""
ENV TARGET_URL ""

# Run console app
CMD ["dotnet", "run", "CosmosTargetConsole.dll"]
