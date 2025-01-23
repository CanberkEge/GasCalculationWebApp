# Gas Calculation Web Application

This is a web application designed to calculate fuel costs based on user inputs and car specifications stored in a SQL database. Users can either manually input their fuel consumption data or select a car from the database to calculate fuel costs.

## Features

- **Manual Fuel Cost Calculation**: Users can input their fuel type, average consumption, and distance to calculate the fuel cost.
- **Car-Based Calculation**: Users can select a car from a database by specifying:
  - Brand
  - Model
  - Generation
  - Year
  - Engine
  - Fuel Type
- **Dynamic Dropdowns**: Dropdowns for car selection dynamically update based on user selections.
- **City and Highway Consumption Display**: Fetches city consumption data for the selected car.
- **Real-Time Fuel Prices**: Scrapes current fuel prices from an external source.

## Technologies Used

- **ASP.NET Core**: Web application framework.
- **Entity Framework Core**: ORM for database interaction.
- **Microsoft SQL Server**: Backend database for storing car data.
- **HTML, CSS, JavaScript**: Frontend design and interactivity.
- **HtmlAgilityPack**: Web scraping library to fetch real-time fuel prices.

## Installation and Setup

 1. **Clone the Repository**:
   ```bash
   git clone https://github.com/your-username/your-repository.git
   cd your-repository
   ```
### 2. Set Up the Database

#### Create the `CarDatas2` Table

Execute the following SQL commands in your SQL Server:

```sql
CREATE TABLE CarDatas2 (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Brand NVARCHAR(50) NOT NULL,
    Model NVARCHAR(50) NOT NULL,
    Generation NVARCHAR(50) NOT NULL,
    Year NVARCHAR(50) NOT NULL,
    Engine NVARCHAR(50) NOT NULL,
    Fuel NVARCHAR(50) NOT NULL,
    HP INT NOT NULL,
    CityConsumption DECIMAL(5, 2) NOT NULL,
    OutsideConsumption DECIMAL(5, 2) NOT NULL
);
```
### 3. Install Dependencies

```dotnet restore```


### 4. Run Migrations
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```


## 5. Start the Application
```
dotnet run
```


## How to Use
- ***Manual Calculation***
- Navigate to the "Manual Input" section.
- Select the fuel type (e.g., Benzin, Dizel, LPG).
- Enter the average gas consumption in liters per 100 km.
- Specify the distance to calculate fuel costs.
- Click Calculate to see the results.
- ***Car-Based Calculation***
- Navigate to the "Car Selection" section.
- Select the car's Brand, Model, Generation, Year, Engine, and Fuel Type using the dropdowns.
- Specify the distance to calculate fuel costs.
- Click Calculate to see the results.



## Contributors

Canberk Ege Erden - https://github.com/CanberkEge