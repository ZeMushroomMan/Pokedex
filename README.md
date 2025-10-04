
# **Pokédex Web Application Documentation**

**Name:** Muhammad Omar  
**Date:** 05/10/2025  
**Case Study Title:** *PokeAPI Coding Project 5*  
**Language Used:** C#, HTML, CSS  
**Framework:** .NET Blazor WebAssembly  

---

## **Document Purpose**
This document outlines the purpose, setup instructions, and usage guide for the Pokédex Web Application. It provides details on how to install, run, and use the application as well as an overview of its features and functionality.

---

## **Overview**
The **Pokédex Web Application** is a modern web app developed using the **.NET Blazor WebAssembly** framework.  
Its primary purpose is to fetch and display Pokémon data from the **PokéAPI** in an interactive and visually appealing interface.

Inspired by the fictional Pokédex from the Pokémon universe, this application allows users to explore, search, and compare Pokémon in a smooth and responsive web experience.

---

## **Features**

### 🧾 **Pokémon List**
- Displays a full list of Pokémon fetched from the PokéAPI.  
- Clicking on a Pokémon name displays detailed information in a card view.

### 🔍 **Search Bar**
- Quickly search Pokémon by name for faster access.

### 🧩 **Filter Menu**
- Filter Pokémon by various criteria including:
  - Type  
  - Ability  
  - Colour  
  - Shape  
  - Habitat  
  - Generation  

### 📊 **Pokémon Details Card**
- Shows detailed information such as:
  - Base stats  
  - Abilities  
  - Height and weight  

### ⚔️ **Fight Button / Comparison Mode**
- Compare two Pokémon based on their base stats and types.  
- After selecting the first Pokémon, a prompt appears to select another for comparison.  
- The app then:
  - Displays both Pokémon’s stats and types.
  - Calculates a total score based on stats and type effectiveness.
  - Declares the Pokémon with the higher score as the winner.
  - Displays “Tie” if both have equal scores.

### 👁️ **View Button**
- Returns to standard Pokémon viewing mode from comparison mode.

### 📱 **Responsive Design**
- Fully optimized for both **desktop** and **mobile** displays.

---

## **Project Setup**

### **Requirements**
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)  
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or Visual Studio Code  
- Internet connection  
- Modern web browser  

---

## **Running Locally**

1. **Clone the repository:**
   ```bash
   git clone https://github.com/ZeMushroomMan/Pokedex
   ```

2. **Open the project** in Visual Studio or VS Code.

3. **Run the project** using the built-in development server.

4. **Access the web app** in your browser at:
   ```
   https://localhost:7176/
   ```

---

## **Accessing the Deployed Version**
The hosted version of the project is available on **Vercel** and can be accessed directly from any modern web browser at:

🔗 **[https://pokedex-hosted.vercel.app/](https://pokedex-hosted.vercel.app/)**

---

## **Technical Details**

### **Comparison Criteria**
- Pokémon are compared using their **base stats** and **type effectiveness**.
- Type advantages are factored in using multipliers based on Pokémon type matchups.
- The total score = *(Base Stats + Type Modifier)*.
- The Pokémon with the higher score wins.
- If both scores are equal, the match results in a tie.

### **API Handling**
- All API responses are **cached in local storage** to minimize redundant requests and improve performance.

### **Error Handling**
- Comprehensive error handling prevents crashes or runtime issues.  
- If errors occur, they are handled gracefully, displaying clear feedback to users.  
- Additional technical details can be viewed in the browser’s **developer console**.

---

## **Developer Information**
**Developed by:** Muhammad Omar  
**Built with:**  
- Blazor WebAssembly  
- C#  
- HTML  
- CSS  
- PokéAPI  

**Hosted on:** [Vercel](https://vercel.com/)  
