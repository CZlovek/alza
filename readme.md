# AlzaShop API

## Overview

AlzaShop API is a robust backend system for managing products in an e-commerce platform. This RESTful API enables core product management operations including searching, viewing details, creating products, and updating stock information. The application is built with modern architectural principles, API versioning, and asynchronous processing capabilities to ensure scalability and maintainability.

## Features

- **Versioned API**: Supports multiple API versions (v1, v2, v3) with backward compatibility
- **Product Management**: Complete CRUD operations for product entities
- **Pagination**: Efficient data retrieval with pagination support (v2/v3)
- **Message Brokering**: Asynchronous command processing with RabbitMQ
- **Swagger Documentation**: Comprehensive API documentation with interactive testing
- **SQLite Database**: Lightweight database with automatic migration and seeding

## Architecture

The project follows a layered architecture with:

- **Controllers**: Handle HTTP requests and route to appropriate services
- **Services**: Implement business logic and data operations
- **Models**: Define the data structure and validation rules
- **Brokers**: Manage asynchronous communication between components

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (latest version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or compatible IDE
- [RabbitMQ Server](https://www.rabbitmq.com/download.html) (for asynchronous messaging)

## Getting Started

### Setup and Configuration

1. **Clone the repository**:
   
```
   git clone https://github.com/CZlovek/alza.git
   cd alza
   
```

2. **Install dependencies**:
   
```
   dotnet restore
   
```

3. **Configure RabbitMQ** (if not using Docker):
   - Ensure RabbitMQ is running on localhost:5672
   - Default credentials are guest/guest

4. **Update connection strings** (if needed):
   - Edit `appsettings.json` to modify database or RabbitMQ connection settings

### Running the Application

#### Using Visual Studio:

1. Open the solution in Visual Studio 2022
2. Set `alza` as the startup project
3. Press F5 to build and run the application

#### Using Command Line:


```
cd alza
dotnet build
dotnet run --project AlzaShopApi

```

The API will be accessible at:
- HTTP: `http://localhost:5167`

Swagger UI is available at: `http://localhost:5167/swagger`

### Docker Support

For containerized deployment:


```
docker-compose up -d

```

Or use the utility script:


```
cmd-docker-install.bat

```

## API Endpoints Overview

### v1 API Endpoints

- `GET /api/v1/Product/{id}`: Retrieve a specific product
- `GET /api/v1/Product`: List all products with availability filtering
- `POST /api/v1/Product`: Create a new product
- `PUT /api/v1/Product`: Update product (synchronous)

### v2 API Endpoints

- `GET /api/v2/Product/{id}`: Retrieve a specific product
- `GET /api/v2/Product?pageIndex=0&pageLimit=10`: List products with pagination
- `POST /api/v2/Product`: Create a new product
- `PUT /api/v2/Product`: Update product (async via internal message broker)

### v3 API Endpoints

- `GET /api/v3/Product/{id}`: Retrieve a specific product
- `GET /api/v3/Product?pageIndex=0&pageLimit=10`: List products with pagination
- `POST /api/v3/Product`: Create a new product
- `PUT /api/v3/Product`: Update product (async via RabbitMQ)

## Running Tests

The project includes extensive unit tests for controllers, services and brokers.

### Using Visual Studio:

1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All Tests" or run specific test classes

### Using Command Line:


```
dotnet test

```

For running specific test classes:


```
dotnet test --filter "FullyQualifiedName~ProductControllerV1Tests"

```
## Utility Scripts

The solution includes several utility scripts to simplify common development tasks:

### cmd-run
These scripts start the application in development mode:
- Launches the application with the appropriate runtime configuration
- Provides a clean console output for monitoring application execution
- Available as `.bat` (Windows) and `.sh` (Unix/Linux/macOS)

### cmd-run-test
These scripts provide a convenient way to execute all tests in the project:
- Automatically runs `dotnet test` to find and execute all tests
- Available as `.bat` (Windows) and `.sh` (Unix/Linux/macOS)
- Clears the console before execution for better readability

### cmd-run-all
These scripts allow parallel execution of multiple commands:
- Launches a series of other batch scripts simultaneously in separate windows
- Supports running different API version commands (v1, v2, v3)
- Available as `.bat` (Windows) and `.sh` (Unix/Linux/macOS)

**Usage:**
- For running tests: execute `cmd-run-test.bat` or `cmd-run-test.sh`
- For running all commands: execute `cmd-run-all.bat` or `cmd-run-all.sh`

These utility scripts help streamline development workflows, save time, and ensure consistency across the development team regardless of operating system.

## Project Structure

- `AlzaShopApi/`
  - `Controllers/`: API endpoints for handling HTTP requests
  - `Models/`: Data models and database entities
  - `Services/`: Business logic implementation
  - `Toolkit/`: Utility classes and broker implementations
  - `Views/`: Data transfer objects for API requests/responses
  - `Database/`: Database context and configuration
- `Tests/`: Unit tests for all components

## Troubleshooting

- **Database Issues**: If database fails to initialize, delete the SQLite file and restart the application
- **RabbitMQ Connection**: Ensure RabbitMQ server is running and connection strings are correct
- **Swagger Not Loading**: Verify XML documentation is being generated in project settings

## License

This project is licensed under the MIT License - see the LICENSE file for details.