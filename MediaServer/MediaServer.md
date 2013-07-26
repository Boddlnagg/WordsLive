#### WordsLive MediaServer
---
# Supported Requests #

This is a list of all supported requests to the media server using HTTP. When a valid URL is requested with an invalid method (not listed below), a `405 Method Not Allowed` response is sent. In the list below, `{...}` is used as placeholder for the name of a directory or file. Likewise, when a URL (path) is requested that is not listed below, a `404 Not Found` response is sent.

## Backgrounds ##
###`GET` /backgrounds/listall###

* **Description:**  
	Retrieves a list of all directories as plaintext, subdiretories and files in these directories. The order of the entries is arbitrary. Entries are separated by line-breaks (one entry per line), and an empty line at the end is allowed. All entries are relative to the root and begin with the `'/'` character. Directories also end with the `'/'` character.
* **Example response content:**  
	`/file.jpg`  
	`/directory/`  
	`/directory/sub/`  
	`/directory/file.jpg`  
	`/directory/video.avi`
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)

###`GET` /backgrounds/list *or* /backgrounds/{...}/list###
* **Description:**  
	Retrieves a list of all directories and files in a specified directory (non-recursively). The order of the entries is arbitrary. Entries are separated by line-breaks (one entry per line), and an empty line at the end is allowed. All entries are relative to the root and begin with the `'/'` character. Directories also end with the `'/'` character.
* **Responses:**
	* `200 OK` (successful)
	* `404 Not Found` (requested directory does not exist)
	* `401 Unauthorized` (if authentication is used)

###`GET` /backgrounds/{...}###
* **Description:**  
	Retrieves the specified file. This can be an image or video. For this request, no authentication is needed!
* **Responses:**
	* `200 OK` (successful)
	* `404 Not Found` (file does not exist)

###`GET` /backgrounds/{...}/preview###

* **Description:**  
	Retrieves a preview image of the specified file. For this request, no authentication is needed!
* **Responses:**
	* `200 OK` (successful)
	* `404 Not Found` (file does not exist)

## Songs

###`GET` /songs/count###

* **Description:**  
	Gets the number of available songs.
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)

###<a id="get-songs-list"></a>`GET` /songs/list###

* **Description:**  
	Gets a listing of the available songs. The response is a JSON array containing objects with the following attributes:
	* Filename: The name of the file (e.g. `Song.ppl`).
	* Title: The title of the song (e.g. `Amazing Grace`).
	* Text: The full text of the song, used for searching. This does not contain chords or translations, but it might contain linebreaks.
	* Copyright: The copyright information, as a single line.
	* Sources: The source information. Mulitple sources are seperated by `';'`.
	* Language: The language of the song.
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)

###`GET` /songs/{...}###
* **Description:**  
	Gets the raw XML data of the specified song file. A successful response comes with a `Last-Modified` header that indicates when the file has been last modified (`PUT`) on the server.
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)
	* `404 Not Found` (song does not exist)

###`PUT` /songs/{...}###
* **Description:**  
	Writes a song to the server. If it already existed, it will be overwritten.
* **Responses:**
	* `200 OK` or `204 No Content` (successful)
	* `401 Unauthorized` (if authentication is used)
	* `500 Internal Server Error` (something went wrong while writing the file)

###`DELETE` /songs/{...}###
* **Description:**  
	Deletes a song from the server.
* **Responses:**
	* `200 OK` or `204 No Content` (successful)
	* `401 Unauthorized` (if authentication is used)
	* `404 Not Found` (song to be deleted does not exist)
	* `500 Internal Server Error` (something went wrong while deleting)

###<a id="get-songs-filter-title"></a>`GET` /songs/filter/title/{...}###

* **Description:**  
	Gets a listing of the available songs whose **title** match a given query. The match is done as an exact case-invariant match, but ignores punctuation and whitespaces other than a single space character. The response is a JSON array as specified in [`GET` /songs/list](#get-songs-list). If no matching songs are found, the response is an emtpy array `[]`.
* **Example:**  
	The query `amazing grace` would match
	* `amazing grace`
	* `Amazing Grace`
	* `Amazing, Grace`
	* `Amazing     Grace`
	* `Amazing Grace How Sweet`  
	It would **not** match
	* `Amazing`
	* `Amazinggrace`
	* `Grace amazing`
	* `Amazing wonderful grace`
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)

###`GET` /songs/filter/text/{...}###

* **Description:**  
	Gets a listing of the available songs whose **text or title** match a given query. The match is done as in [`GET` /songs/filter/title/{...}](#get-songs-filter-title) and linebreaks in the text are ignored. The response is a JSON array as specified in [`GET` /songs/list](#get-songs-list).
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)

###`GET` /songs/filter/source/{...}###

* **Description:**  
	Gets a listing of the available songs whose **source information** match a given query. The match is done as in [`GET` /songs/filter/title/{...}](#get-songs-filter-title). The response is a JSON array as specified in [`GET` /songs/list](#get-songs-list).
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)

###`GET` /songs/filter/copyright/{...}###

* **Description:**  
	Gets a listing of the available songs whose **copyright information** match a given query. The match is done as in [`GET` /songs/filter/title/{...}](#get-songs-filter-title). The response is a JSON array as specified in [`GET` /songs/list](#get-songs-list).
* **Responses:**
	* `200 OK` (successful)
	* `401 Unauthorized` (if authentication is used)

###`GET` /bible/{...}###

**TODO**

###`GET` /files/{...}###

**TODO**

##PHP MediaServer##
You can use a standard PHP-enabled (shared) web server as MediaServer using [this script](TODO). Put the files `index.php` and `.htaccess` alongside each other somewhere on your server. To correctly configure passwort protection, you have to  edit `.htaccess` and set the path to your `.htdigest` file with the encrypted password (the user must always be WordsLive). In the first lines of index.php you can also configure the directories used for songs and backgrounds. Just put your files in the directories you specify there and make sure that the PHP user can write to the songs directory. You might need `chown` to do this. You can test your server by pointing your web browser to any of the `GET`-URLs listed above, for example `http://your.server.com/songs/count`. It should request a password from you if you use one and show the number of songs.