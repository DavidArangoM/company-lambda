FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base

# Set environment variables
# ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
# ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

RUN apt-get update && apt-get install -y zip

WORKDIR /src
COPY ["src/AWSLambda.Collection.SendEmailSES/AWSLambda.Collection.SendEmailSES.csproj", "AWSLambda.Collection.SendEmailSES/"]
COPY ["src/AWSLambda.Collection.SendEmailSES.BL/AWSLambda.Collection.SendEmailSES.BL.csproj", "AWSLambda.Collection.SendEmailSES.BL/"]
COPY ["src/AWSLambda.Collection.SendEmailSES.Infra/AWSLambda.Collection.SendEmailSES.Infra.csproj", "AWSLambda.Collection.SendEmailSES.Infra/"]

RUN dotnet restore "AWSLambda.Collection.SendEmailSES/AWSLambda.Collection.SendEmailSES.csproj" -p:PublishReadyToRun=true -p:PublishTrimmed=true
COPY . .
WORKDIR "/src/AWSLambda.Collection.SendEmailSES"
#RUN dotnet build "AWSLambda.Collection.SendEmailSES.csproj" --configuration Release --output /app/build
RUN dotnet publish "AWSLambda.Collection.SendEmailSES.csproj" --configuration Release --output /app/publish -p:PublishReadyToRun=false

RUN mkdir /build-artifacts && \
    mv /app/publish/* /build-artifacts && \
    cd /build-artifacts && \
    zip -r lambda.zip .

VOLUME /deploy

CMD ["cp", "-r", "/build-artifacts/lambda.zip", "/deploy/"]

# sudo docker build -t my-csharp-compiler .
# sudo docker run --rm -v $(pwd)/deploy:/deploy my-csharp-compiler
# docker volume rm -f 890db39cd16079768cdae0008b881da1ac375d491e911e063063139f52ad79f2