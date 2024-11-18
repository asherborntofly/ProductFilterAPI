# Product Filter API

## Solution Overview

The Product Filter API is designed to provide a robust and efficient way to filter products based on various query parameters. The solution incorporates several best practices to ensure clean, readable, and maintainable code while addressing performance, scalability, security, and user documentation.

### Key Features of the Solution

1. **Clean, Readable, Easy-to-Understand Code**:

   - The code is structured using clear naming conventions and follows the Single Responsibility Principle. Each method and class has a specific purpose, making it easier for developers to understand and maintain the codebase.

2. **Performance, Scalability, and Security**:

   - **In-Memory Caching**: Implemented in-memory caching using `IMemoryCache` to store filtered product results. This reduces the number of external API calls, improving response times and overall performance.
   - **Asynchronous Programming**: Utilized asynchronous programming patterns (async/await) to handle I/O-bound operations efficiently, allowing the application to scale better under load.
   - **Input Validation**: Ensured that all user inputs are validated to prevent common security vulnerabilities such as SQL injection and XSS attacks.
   - **HTTPS**: The API is designed to be served over HTTPS, ensuring that data in transit is encrypted.

3. **Unit Tests**:

   - Unit tests are implemented to verify the functionality of the API endpoints and the caching logic. This ensures that changes to the code do not introduce regressions and that the application behaves as expected.

4. **Dependency Injection**:

   - The application uses dependency injection to manage service lifetimes and dependencies. This promotes loose coupling and enhances testability, allowing for easier unit testing and maintenance.

5. **Appropriate Logging**:

   - Integrated logging using Serilog to capture important events and errors. The logging includes detailed information about the external API responses, which aids in debugging and monitoring the application’s behavior.
   - Example of a logged response from the mocky.io API:
     ```
     {
       "products": [
         {
           "id": 1,
           "name": "Product A",
           "price": 10.99,
           "sizes": ["small", "medium"]
         },
         {
           "id": 2,
           "name": "Product B",
           "price": 15.49,
           "sizes": ["medium", "large"]
         }
       ]
     }
     ```

6. **Documentation for Users of the API**:
   - Comprehensive documentation is provided in the README file, detailing the API endpoints, request formats, and response structures. This documentation helps users understand how to interact with the API effectively.
   - Example requests and responses are included to illustrate how to use the filtering capabilities of the API.

### Getting Started

To get started with the Product Filter API, follow the installation and usage instructions provided in this README. Ensure that you have the necessary prerequisites installed, including the .NET 8 SDK and a suitable code editor.

By following these practices, the Product Filter API is designed to be a high-performance, scalable, and secure solution that is easy to understand and maintain.

## Overview

The Product Filter API is a .NET 8 Web API that provides an HTTP endpoint to filter products based on various query parameters. The API retrieves product data from a mock external source and allows users to filter products by price, size, and highlight specific words in product descriptions.

## Features

- **GET /products/filter**: Accepts optional query parameters to filter products:

  - `minprice`: Minimum price of the products.
  - `maxprice`: Maximum price of the products.
  - `size`: Size of the products (e.g., small, medium, large).
  - `highlight`: Comma-separated words to highlight in product descriptions.

- Returns a JSON response containing:

  - All products if no parameters are provided.
  - A filtered subset of products based on the provided parameters.
  - A filter object with:
    - Minimum and maximum price of all products.
    - An array of distinct sizes available.
    - An array of the ten most common words in product descriptions, excluding the five most common.

- Highlights specified words in product descriptions using HTML `<em>` tags.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A code editor (e.g., Visual Studio, Visual Studio Code)

### Installation

1. Clone the repository:

   ```bash
   git clone <repository-url>
   cd ProductFilterApi
   ```

2. Restore the dependencies:

   ```bash
   dotnet restore
   ```

3. Run the application:

   ```bash
   dotnet run
   ```

4. The API will be available at `http://localhost:5000/products/filter`.

### Example Requests

- **Get all products**:

  ```http
  GET /products/filter
  ```

- **Filter products by price**:

  ```http
  GET /products/filter?minprice=5&maxprice=20
  ```

- **Filter products by size**:

  ```http
  GET /products/filter?size=medium
  ```

- **Highlight words in descriptions**:

  ```http
  GET /products/filter?highlight=green,blue
  ```

### Response Format

The API returns a JSON response structured as follows:
