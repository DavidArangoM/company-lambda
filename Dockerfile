FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base

# Set environment variables
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

WORKDIR /src
COPY ["src/AWSLambda.Collection.SendEmailSES/AWSLambda.Collection.SendEmailSES.csproj", "AWSLambda.Collection.SendEmailSES/"]
COPY ["src/AWSLambda.Collection.SendEmailSES.BL/AWSLambda.Collection.SendEmailSES.BL.csproj", "AWSLambda.Collection.SendEmailSES.BL/"]
COPY ["src/AWSLambda.Collection.SendEmailSES.Infra/AWSLambda.Collection.SendEmailSES.Infra.csproj", "AWSLambda.Collection.SendEmailSES.Infra/"]

RUN dotnet restore "AWSLambda.Collection.SendEmailSES/AWSLambda.Collection.SendEmailSES.csproj" -p:PublishReadyToRun=true -p:PublishTrimmed=true
COPY . .
WORKDIR "/src/AWSLambda.Collection.SendEmailSES"
RUN dotnet build "AWSLambda.Collection.SendEmailSES.csproj" --configuration Release --output /app/build

FROM base AS publish
RUN dotnet publish "AWSLambda.Collection.SendEmailSES.csproj" --configuration Release --output /app/publish -p:PublishReadyToRun=false

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS final
WORKDIR /var/task
ENV DOTNET_CLI_HOME=/tmp/.dotnet
COPY --from=publish /app/publish .
CMD ["AWSLambda.Collection.SendEmailSES::AWSLambda.Collection.SendEmailSES.Function::FunctionHandler"]