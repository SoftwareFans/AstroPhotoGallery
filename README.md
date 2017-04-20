# Astronomy gallery

## April 20th 2017:

The project was part of the course "Software technologies" in Software University Bulgaria. It has been developed by 2 software development students with less than a year experience. The inspiration came a website for astronomy pictures: http://www.emilivanov.com/index2.htm. The right to use the pictures for this project has been given by Emil Ivanov via email and the project will be presented to him too. This project will not be used for any commercial purposes - it is a non-profit student project.

Due to the small experience of the developers when the project was created, it does not pretend to be perfect from software engineering point of view. However, many hours have been dedicated to it in order to work, look and feel good.

The beautiful pictures are separated by categories. They can be searched by tags or by categories.

More details and features:

- Authentication: Users can register and log in to the site with email (username) and password
- Seed at first startup: Default administrator user account "admin@astrogallery.net", roles "Admin" and "User" and category "Other" are being created during the first start of the project 
- Upload: Pictures can be uploaded and saved in the project's database (by a logged in user only)
- Download: Pictures can be downloaded to a user's PC
- Categories: Every uploaded picture is put in a category
- Edit: Pictures can be edited (title, description, category) by administrators or by the picture's uploader
- Security: Categories can be created and edited/deleted only by administrators
		* If a whole category is deleted then all of the pictures in that category are deleted too
- Tags: Every uploaded picture is required to has at least one tag for it
- Display modes: The pictures can be displayed by tags
- Display modes: The pictures can be displayed by categories
- Browsing: When looking at a picture the user can go to the next/previous picture in that category (if there is such)
- Browsing: When looking at a picture the user can go to the category of the picture
- Browsing: When looking at a picture the user can see the user profile of the uploader of the picture
- Image validation: Image file extension and content validation - when a picture is being uploaded (valid for profile picture too) its content is being checked in order to be determined if it is really an image. That way even if a user change the extension of a .txt file for example to .jpg the application will reject the file and won't store it anywhere.
- Validations and notifications: If an invalid data is entered by a user then a notification is shown saying what the error is. Bootstrap notifications are used for that feature.
- Search: Search feature for categories is included in the navigation bar (case-insensitive)
- Search: Search feature for categories is included in the administrator panel
- Search: Search feature for users is included in the administrator panel
- Paging: If the number of categories is higher than a certain value then the categories are being showed on more than one page
- Paging: If the number of users is higher than a certain value then the users in the administrator panel are being showed on more than one page
- Slider: On the index page of the project there is a slider showing a picture from each possible category (e.g. one planet, one galaxy, one nebulae, one star cluster etc.). The pictures in the slider are static and not taken from the currently uploaded pictures by the users because there can be category with no pictures in it
- User profiles: Every user can edit her/his user profile – e.g. gender, city, country, birthday, profile picture
	* The profile picture of a user is saved in a subdirectory with name the user ID in directory ~/Content/images/profilePics
	* The birthday date of the user profile is being selected using Javascript date picker
- Administrator panel: Users in role "Admin" have access to administrator panel giving them rights to edit/delete user accounts (make them administrators too) and edit/delete categories



