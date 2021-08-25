# Smart-backup


## Table of contents
* [General info](#general-info)
* [Technologies](#technologies)
* [Setup](#setup)

## General info
This windows app backs up all databases on servers.Connects to all servers on schedule , backs up all specific databases .Then creates zip file and copies them to the destination.The application has developed in 2012.
	
## Technologies
Project is created with:
* C#.NET
* SQL Server
* xp_CMDShell in SQL
	
## Setup
To run this project, clone the repository in your local repo:

```
$ run sql script to create database
$ input servers ip in database
$ run .exe
