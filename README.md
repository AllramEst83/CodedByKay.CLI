# CodedByKay.CLI

SmartDialogueCLI is a command-line application designed to manage and facilitate smart dialogues and assistants. It offers a simple yet powerful interface for users to interact with smart dialogue systems and integrate assistant functionalities seamlessly.

## Core Features

- **Smart Dialogue Management**: Execute and manage dialogues with smart logic integration.
- **Assistants Integration**: Easily integrate and manage assistant functionalities to enhance user interactions.
- **Error Handling**: Robust error handling for a smoother user experience, providing clear feedback for command-line inputs.
- **Customizability**: Offers customizable options to tailor the dialogue and assistant features according to user needs.

## Quick Start

- Install the application by cloning the repository and building the project.
- Run `SmartDialogueCLI` with the desired options to start managing your dialogues and assistants.

For detailed instructions and more information, refer to the installation and usage sections.

## Contributing

Your contributions are welcome! Please feel free to fork the repository, create your feature branch, and submit a pull request.

## License

This project is licensed under the    Apache License Version 2.0 - see the LICENSE file for details.

# How to Access Your Console Application with a Custom Command on Windows 11

This guide will walk you through the process of compiling a .NET console application, renaming it for easy access via a custom command (`smartd`), and configuring your system to recognize this command from any command prompt or PowerShell window.

## Prerequisites

- Ensure you have .NET 8.0 SDK installed on your Windows 11 machine to compile the console application.
- Your console application's source code.

## Step 1: Compile the Console Application

1. **Open Command Prompt or PowerShell**: Navigate to the directory containing your project file (`.csproj`).

2. **Publish Your Application**: Execute the following command, adjusting the runtime identifier (`-r`) if necessary for your target architecture (e.g., `win-x64` for 64-bit Windows):
    ```shell
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
    ```
   This command compiles your application into a single executable file, including all necessary dependencies.

## Step 2: Rename the Executable (Optional)

- Find the published executable in your project's `bin\Release\net8.0\win-x64\publish\` directory.
- Rename the executable to `smartd.exe` for ease of use.

## Step 3: Make the Command Accessible System-wide

1. **Choose a Suitable Location**: Move `smartd.exe` to a dedicated directory for your tools, such as `C:\devtools\`.

2. **Update the System PATH**:
   - Right-click on the Start button and select `System`.
   - Click on `Advanced system settings` → `Environment Variables`.
   - Under "System variables," find and select `Path`, then click `Edit`.
   - Click `New` and enter the path to the directory containing `smartd.exe` (e.g., `C:\devtools\`).
   - Click `OK` on all dialogs to save your changes.

## Step 4: Test the Command

- Open a new Command Prompt or PowerShell window.
- Type `smartd` and press Enter. If everything is set up correctly, your application should run, indicating that `smartd` is now recognized as a command.

## Tips for Distribution

- If you plan to share your `smartd` tool, consider creating a simple installer script that automates the steps above, especially updating the PATH variable.
- Include clear instructions for users on how to undo the changes, should they wish to remove the command from their system.
