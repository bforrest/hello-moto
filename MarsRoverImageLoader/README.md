# Programming Exercise

Use the NASA API described [here](https://api.nasa.gov) to build a project that calls the Mars Rover Photos API with a given day as input, returning corresponding photo images as output. The application should download and store each image locally. The submission of the project should be via GitHub.

## Acceptance Criteria

- when complete, please send a link to your own GitHub repository in an email replying to the email from which you received this exercise (or to watchguard.eng.account.owner@motorolasolutions.com)
- you are encouraged to think of this as an incremental exercise (e.g., reply back when you have implemented the basic acceptance criteria, then again if you make further improvements or implement one or more of the bonuses)
- use the list of dates below, stored in text file "dates.txt", to pull the images that were captured on that date, by reading dates one-by-one from the file:
    - 02/27/17
    - June 2, 2018
    - Jul-13-2016
    - April 31, 2018
- language should be C#/.NET Core on the backend, and (if applicable) your choice of JavaScript framework on the frontend
- the project should run and build locally, after you submit it (each time you submit it)
- include relevant documentation (e.g., .MD file) in the repository

## Bonuses

- Bonus: unit tests, static analysis, performance tests, or any other things you feel are important to meet Acceptance Criteria for Definition of Done
- Double Bonus: have the application display the image in a web browser
- Triple Bonus: have the application run in a Docker container


## Barry's notes:

- Added the dates.txt file to the csproj file and set it to copy to the output directory. Then when I decided to create the Photos folder, I used the Parent.Parent.Parent hack to get to the project directory instead.
- Set an environment variable for the API_KEY value. I hear that nasty things happen when keys are stored in source control and most folks have them as variables withing their CI/CD 
- I learned a *cheat* to convert [JSON to C#][https://json2csharp.com/] so I used Postman to get a sample of the JSON response and fed it to the transmorgifier to get a DTO class for my model.