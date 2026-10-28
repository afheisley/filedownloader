# filedownloader
this is a file downloader made in .net

## Overview
Filedownloader is a simple Windows desktop application built with C# and WinForms. It allows users to paste a file URL, choose a save location, and download the file to their computer. The app includes progress tracking, cancellation support, and safe handling for overwrite scenarios.

## Features
- Enter an HTTP or HTTPS download link
- Choose a destination file path
- Automatically suggest a filename from the URL when possible
- Validate the link and save folder before starting the transfer
- Download files in the background using `HttpClient`
- Display progress, file size, and status updates
- Cancel an active download at any time
- Save to a temporary file first, then move it into place when complete
- Prompt before replacing an existing file

## Purpose
This project is a lightweight, user-friendly file downloader for Windows desktop use, designed to make downloading a single file from the web quick and straightforward.