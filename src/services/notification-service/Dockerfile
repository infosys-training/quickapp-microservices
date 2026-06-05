FROM eclipse-temurin:21-jdk AS build
WORKDIR /src
COPY pom.xml .
COPY shared/shared-contracts/pom.xml shared/shared-contracts/pom.xml
COPY shared/shared-infrastructure/pom.xml shared/shared-infrastructure/pom.xml
COPY api-gateway/pom.xml api-gateway/pom.xml
COPY services/identity-service/pom.xml services/identity-service/pom.xml
COPY services/customer-service/pom.xml services/customer-service/pom.xml
COPY services/order-service/pom.xml services/order-service/pom.xml
COPY services/product-service/pom.xml services/product-service/pom.xml
COPY services/notification-service/pom.xml services/notification-service/pom.xml
RUN apt-get update && apt-get install -y maven && rm -rf /var/lib/apt/lists/*
RUN mvn dependency:go-offline -pl services/notification-service -am -B
COPY . .
RUN mvn -pl services/notification-service -am package -DskipTests -B

FROM eclipse-temurin:21-jre
WORKDIR /app
COPY --from=build /src/services/notification-service/target/*.jar app.jar
EXPOSE 5005
ENTRYPOINT ["java", "-jar", "app.jar"]
