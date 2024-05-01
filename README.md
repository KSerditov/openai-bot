# Simple Telegram Proxy Bot for OpenAI API

This project is a lightweight and simple Telegram proxy bot that forwards client requests to the OpenAI API and sends the responses back to the client.

The bot does not maintain chat context between messages. Each client message is treated as a new, unrelated request.

## Deployment

The bot can be deployed on low-end VPS platforms, such as [Gullo](https://hosting.gullo.me/).

### Prerequisites

- **.NET Runtime 7**: The bot requires the .NET Runtime 7. Installation instructions can be found [here](https://learn.microsoft.com/en-us/dotnet/core/install/linux).

### Configuration

Ensure to set up the OpenAI API key and the Telegram bot key in the service configuration file as in this [example](https://github.com/KSerditov/openai-bot/blob/main/telegram-bot-openai.service).

## Usage

Here's how you can interact with the bot:

![image](https://github.com/KSerditov/openai-bot/assets/3009597/8b8488fe-fd35-4e76-b3e0-371bc13e71fd)


## Continuous Integration and Deployment
The repository includes a basic CI/CD pipeline using GitHub Actions. It builds the application and deploys it to a remote server whenever changes are merged into the main branch.
