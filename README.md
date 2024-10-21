## Running the Project

To get started, follow these steps:

1. **Navigate to the Project Directory**:  
   Open your terminal and change to the directory containing the `docker-compose.yml` file (**Uttom.Infrastructure**).

2. **Start the Containers**:  
   Execute the command:
   ```bash
   docker compose up -d
   
This command will pull the necessary dependencies and start all defined containers.

### Access the API:
Once the containers are running, you can launch the **Uttom.API** project. The **Swagger UI** will open automatically, allowing you to explore and interact with various API endpoints.

### Accessing External Services

- **MinIO storage** is available at:  
  [http://localhost:9090/browser/](http://localhost:9090/browser/)

- **RabbitMQ** is available at:  
  [http://localhost:15672/#/](http://localhost:15672/#/)

All services use default credentials. Check the `docker-compose` file to retrieve them.

### Tools Used

The project makes use of the following tools and libraries:

- **FluentValidation**
- **MediatR**
- **OpenApi**
- **EntityFrameworkCore**
- **Inflector**
- **MassTransit**
- **RabbitMQ**
- **xUnit**
- **Npgsql.EntityFrameworkCore.PostgreSQL**
- **Testcontainers**
- **FluentAssertions**
- **Testcontainers.Minio**
- **Testcontainers.PostgreSql**
- **Testcontainers.RabbitMq**
- **NSubstitute**



### Technical Information
The project has over 80 tests (many more can be added), which include:

- **Integration Tests**:  
  Utilizing **TestContainers** to spin up instances of **PostgreSQL**, **RabbitMQ**, and **MinIO** for storage during testing.

- **Unit Tests**:  
  Implementing **NSubstitute** for mocking **RabbitMQ** interactions and using an in-memory database for persistence.



