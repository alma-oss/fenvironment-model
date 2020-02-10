FROM dcreg.service.consul/dev/development-dotnet-core-sdk-common:3.1

# build scripts
COPY ./fake.sh /environment-model/
COPY ./build.fsx /environment-model/
COPY ./paket.dependencies /environment-model/
COPY ./paket.references /environment-model/
COPY ./paket.lock /environment-model/

# sources
COPY ./EnvironmentModel.fsproj /environment-model/
COPY ./src /environment-model/src

# others
COPY ./.git /environment-model/.git
COPY ./CHANGELOG.md /environment-model/

WORKDIR /environment-model

RUN \
    ./fake.sh build target Build no-clean

CMD ["./fake.sh", "build", "target", "Tests", "no-clean"]
