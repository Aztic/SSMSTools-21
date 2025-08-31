# SSMS-Tools 21
Set of tools to improve my own experience using SSMS. Intended to be used with the newer version of SSMS (21).
Separated in a different repository from [SSMS Tools](https://github.com/Aztic/SSMS-Tools) since the shell changed and now Visual Studio 2022 can be used for development.

## Requirements
- SQL Server Management Studio 21

## Tools
- Multi Db Query runner

[Demo](https://github.com/user-attachments/assets/268998ef-60fb-4b83-ab3c-4ce822cfeb95)

Generates a query to be executed in the selected databases of the server

# Setup
- Download the latest zip file with the content
- Go to your install path and its `Extensions` subfolder (for example: `C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions`)
- Create a `SSMSTools_21` folder
- Paste the content of the zip file inside the folder

## Development

### Setup
Inside the solution, you need to change two paths:
- Debug -> External program being started
- VSIX -> VSIX content copy path

Both have paths that, although common, might differ from your local development

## TODO
- Improve UI / UX
- Pipeline
- In-Memory multi db runner command to display the result

## License
MIT
