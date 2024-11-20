# Warehouse API

Warehouse API is a RESTful service for managing products in a warehouse. It supports CRUD operations and fuzzy search functionality.

## Table of Contents

- [Warehouse API](#warehouse-api)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Installation](#installation)
  - [Usage](#usage)
  - [API Endpoints](#api-endpoints)
    - [Products](#products)
  - [Configuration](#configuration)

## Features

- CRUD operations for products
- Fuzzy search with customizable options
- Entity Framework Core integration
- MySQL and InMemory database support

## Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/svenniz/spac-warehouse.git
    cd spac-warehouse/WarehouseApi
    ```

2. Install dependencies:
    ```sh
    dotnet restore
    ```

3. Update the database connection strings in `appsettings.json` and `appsettings.Development.json`.

4. Apply migrations:
    ```sh
    dotnet ef database update
    ```

5. Run the application:
    ```sh
    dotnet run
    ```

## Usage

The API can be accessed at `http://localhost:5035` (or the configured URL). Swagger UI is available at `http://localhost:5035/swagger`.

## API Endpoints

### Products

- **Get all products**
    ```http
    GET /api/product
    ```

- **Get a product by ID**
    ```http
    GET /api/product/{id}
    ```

- **Create a new product**
    ```http
    POST /api/product
    ```

- **Update a product**
    ```http
    PUT /api/product/{id}
    ```

- **Delete a product**
    ```http
    DELETE /api/product/{id}
    ```

- **Search products**
    ```http
    GET /api/product/search?query={query}&FuzzyLevel={level}&IgnoreCase={true|false}&IgnoreDuplicates={true|false}&IgnoreLength={true|false}&IgnoreCommonTypos={true|false}&Name={true|false}&Description={true|false}
    ```

## Configuration

Configuration settings can be found in `appsettings.json` and `appsettings.Development.json`. Update the connection strings and other settings as needed.

