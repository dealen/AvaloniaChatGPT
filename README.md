# AvaloniaChatGPT
Small app for learning purposes.

### Goal

I was thinking about writing small desktop app to use OpenAI API which could:
- moderation of calls to OpenAI API
- have system message
- settings for API
    - temperature
    - maximum length
    - model - probably only gpt3.5-turbo and gpt4 (or newer if will be available)
    - rest of options when I will find time for them
- history in db or files
    - json
        - import json
        - json functionality upgrade (now it is possible to save to json but to desktop, without possibility to choose destination)
- exception habndling
- unit/integration tests 

Right now there is only call for api with question and not much more. 
