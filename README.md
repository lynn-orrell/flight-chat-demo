# Flight Chat Demo

## Overview

Flight Chat Demo is an interactive application that allows users to query and analyze flight, airport, and weather data using natural language. The project demonstrates how to combine conversational AI with real-world datasets and OData APIs for rich, contextual insights about flights in the United States.

## Installation & Setup

1. **Clone the repository**
   ```sh
   git clone <this-repo-url>
   ```
2. **Navigate to the project directory**
   ```sh
   cd flight-chat-demo
   ```
3. **Unzip Data Files**
   - Go to `FlightInfoService/DataFiles/`.
   - The directory contains multipart zip files: `DataFiles.zip.001`, `DataFiles.zip.002`, `DataFiles.zip.003`.
   - Use a tool like [7-Zip](https://www.7-zip.org/) or similar to extract the contents:
     - Right-click on `DataFiles.zip.001` and select "Extract here" (the tool will automatically use all parts).
   - This will extract the required CSV data files for airports, flights, and weather.

4. **Open the Project**
   - **Visual Studio:**
     - Open `flight-chat.sln`.
   - **VS Code:**
     - Open the root folder (`flight-chat-demo`).
     - Make sure you have the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) or [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) installed.

5. **Restore dependencies**
   - In either environment, restore NuGet packages:
     - **Visual Studio:** This happens automatically on build.
     - **VS Code:**
       - If you have the C# Dev Kit extension installed, NuGet packages will be restored automatically.
       - Otherwise, you can restore manually in the terminal:
         ```sh
         dotnet restore
         ```

## Project Structure

- **FlightInfoService**: Provides the data API for flights, airports, and weather. This service must be running to supply data. 
  - Note: FlightInfoService may take some time to start up due to the large volume of flight data it exposes (all US flights from 2023).
- **FlightChat**: Provides the chat interface that uses AI to query the data from FlightInfoService. This service must also be running for the full demo experience.

## Running the Demo

To get the full value from the demo, you need to run both FlightInfoService (the data provider) and FlightChat (the AI chat interface).

### Visual Studio

1. Set `FlightInfoService` as the startup project and run (F5 or Ctrl+F5).
2. In a new Visual Studio instance or after the first is running, set `FlightChat` as the startup project and run.

### VS Code / Terminal

- A `.vscode/launch.json` file is included with pre-configured debug launch settings for both `FlightInfoService` and `FlightChat`. You can use the Run and Debug panel in VS Code to start either project directly.

1. Open the Run and Debug panel (Ctrl+Shift+D) in VS Code.
2. Select **C#: FlightInfoService Debug** and start it.
3. Open a new debug session, select **C#: FlightChat Debug**, and start it as well.

Alternatively, you can run both from the terminal in separate terminals:

1. To run the FlightInfoService:
   ```sh
   cd FlightInfoService
   dotnet run
   ```
2. To run the FlightChat project:
   ```sh
   cd ../FlightChat
   dotnet run
   ```

## Example Natural Language Flight Queries

Try these sample requests in the chat interface or API:

- "Show me the list of airline companies."
- "Which flights from OKC were delayed due to weather last December?"
- "What was the average departure delay for flights from JFK to LAX in March?"
- "Can you show that in a graph by airline?"

Feel free to experiment with your own queries about flights, airports, or weather conditions!
