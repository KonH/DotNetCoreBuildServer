# DotNetCoreBuildServer

**DotNetCoreBuildServer** is a build system based on **.NET Core** (netstandard1.4). It allows you to run batch of actions (check/copy directory/files, run external tools) and collect results in a simple way. Also, it provides **Slack** integration.

## Configuration

### Project config

Server config based on **.json** file in given format:

```
{
	"builds": "pathToBuildConfigDirectory"
}
```

If you need Slack integration, add these lines:

```
{
	...
	"slack_token": "bot_token",
	"slack_hub": "channel_or_user_name"
}
```

Slack token you can get in team dashboard after adding new bot, slack hub can be #channelName or @userName, and after that you can control your bot in this channel or direct messages with user.

Any other parts of project config are optional and used as replacers. For example, you have "root" in project config:

```
{
	...
	"root": "pathToYourProject"
}
```

After it, you can use {root} in build tasks:

```
{
	"path": "{root}/subDirectory"
}
```

### Build config

Build configs are placed to one directory, specified in "builds" in project config. One **.json** file per build.

Build config looks like this:

```
{
	"args": [
		"arg_0", "arg_1", ... "arg_N"	
	],
	"tasks": [
		{
			"task_name": {
				"command_name": {
					"command_arg_0": "value_0",
					"command_arg_1": "value_1",
					...
					"command_arg_N": "value_N"
				}
			}
		}
	]
}
```

Any command has "message" and "result", "message" is full output, "result" can contain short info (see "run" task). You can get these values using **{taskName:message}** and **{taskName:result}** in next tasks parameter values.

Full list of commands you can see at **Tasks** section.

Also, you can look at full example [here](https://github.com/KonH/DotNetCoreBuildServer/blob/master/ConsoleClient/bin/Debug/netcoreapp1.1/buildConfigs/dev_build.json). 

## Start

You need to [dotnet](https://www.microsoft.com/net/download/core) runtime installed. Next steps is osX based, but Windows is also supported:

```
git clone https://github.com/KonH/DotNetCoreBuildServer.git
cd DotNetCoreBuildServer
dotnet restore
dotnet publish -c RELEASE
```

Next, you need to run server:

```
cd ConsoleClient/bin/Release/netcoreapp1.1/
dotnet ConsoleClient.dll serverName configs
```

Where:

* serverName - your server name
* configs - path to at least one **.json** project config

After that, your server is ready to use and works before you decide to stop it.

## Usage

You can control your server with commands described below:

```
- "status" - current server status
- "stop" - stop current build, if it is started
- "build arg0 arg1 ... argN" - start build with given parameters
- "help" - show this message
```

## Build process

Build tasks executed in described order, if one task failed, all next tasks are skipped. If build is done, last "message" is shown. If build is failed, full execution log is shown.

## Slack integration usage

Using Slack integration, you can execute the same commands, when you mention your bot (at first word int your message). So, if you write in specific room:

```
@buildbot build myBuild 1.0.0
```
It is recognized as:

```
build myBuild 1.0.0
```

And build is started (if exists).

In any server response you can see server name:

```
konh [5:55 PM] 
@buildbot status

buildbotAPP [5:55 PM] 
[testServer]
 Server 0.1.0.0
Is busy: True
Services:
- ConsoleService
- SlackService
Builds:
- dev_build (tag)
```

You can use several servers (as many as you want), but if you don't need to duplicate builds, use unique build names at those servers (e.g. two servers: 'osxServer' and 'winServer', with builds: ['osx.x64'] and ['win.x32', 'win.x64'] respectively).

## Limitations

* One build per time
* You can stop current build on server, but it actually stops after current task is ended

## Tasks

### Run

- Command output is shown as "message"
- If "logfile" exists, "message" does not contain actual message, but contains path to log
- To catch errors in output, use "error_regex"
- To convert message to short "result", use "result_regex" 

```
{
	"task_name": {
		"run": {
			"path": "path_or_command",
			"args": "arguments",
			"work_dir": "work_directory",
			"error_regex": "regex_to_catch_errors",
			"log_file": "path_to_logfile"
		}
	}
}
```
Examples:

```
{
	"fetch_last_repo": {
		"run": {
			"path": "git",
			"args": "fetch --all",
			"work_dir": "{root}/git_repo",
			"error_regex": "(error|fatal)"
		}
	}
}
```

```
{
	"upload_build": {
		"run": {
			"path": "svn",
			"args": "commit -m \"Build for {tag}.\"",
			"work_dir": "{root}/local_svn",
			"error_regex": "(error|Commit failed)",
			"log_file": "{root}/upload_log.txt",
			"result_regex": "(?<=Committed revision )([0-9]*)"
		}
	}
}
```

### Print

```
{
	"task_name": {
		"print": {
			"message": "some_text"
		}
	}
}
```

Example:

```
{
	"build_report": {
		"print": {
			"message": "Build result rev.{upload_build:result}"
		}
	}
}
```

You can place this task as last place and collect all required data in its "message".

### Check file exist/check dir exist

Failed, if file doesn't exist

```
{
	"task_name": {
		"check_file_exist": {
			"path": "path_to_file"
		}
	}
}
```
```
{
	"task_name": {
		"check_dir_exist": {
			"path": "path_to_dir"
		}
	}
}
```


### Delete file/delete dir

Failed only if "if_exist": "true"

```
{
	"task_name": {
		"delete_file": {
			"path": "path_to_file",
			"if_exist": "true"
		}
	}
}
```
```
{
	"task_name": {
		"delete_dir": {
			"path": "path_to_dir",
			"if_exist": "true"
		}
	}
}
```

### Copy file/copy dir

```
{
	"task_name": {
		"copy_file": {
			"from": "fromPath",
			"to": "toPath"
		}
	}
}
```
```
{
	"task_name": {
		"copy_dir": {
			"from": "fromPath",
			"to": "toPath"
		}
	}
}
```