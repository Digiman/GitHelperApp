# GitHelperApp

## Introduction

The tool created specially for managing the complexity with identifying changes in many repositories. Because we have a lot of repositories to
manage and also for releases we need to create the Pull Requests we have got an idea to build the automation tool.
First idea was about identify the changes for repositories to be able to get the list of them.
Second idea after getting the result of comparison - create Pull request automatically.

So the basic use cases and workflow to use the tool:

1. Create PRs for release from Dev to Release branches.
2. Create PRs after release from Release to Main branches.
3. Create PRs after release from Main to Dev branches -to sync the work items.

Currently configuration of the tool support next ways to compare:

1. Dev to Release (DR).
2. Release to Main (RM).
3. Main to Dev (MD).
4. Release to Dev (RD).

This set in the separate configuration files.

## How to use

There are some special functionality in the application to work with the repositories.

Now it’s available some special commands and actions:
1. Compare branches - with local repositories and can be on Azure DevOps.
2. Search Pull Requests - search for PRs available for repositories.
3. Create Pull Requests - automatically search and compare repositories and create PR where changes exists.
4. Search Work Items - search for work items in repositories with the changes (based on commits difference).
5. Get list of all repositories in the team project.

More details provided for each command available in the application.

Each operations build as Command in the code. It can be simple to add a new command and logic because applications is very
extendible.

For each command already added simple launch profile in the launchSettings.json file so it can be just selected right one and choose the
environment name (config to use).

```json
{
    "$schema": "http://json.schemastore.org/launchsettings.json",
    "profiles": {
        "GitHelperApp": {
            "commandName": "Project",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "Local"
            }
        },
        "GitHelperApp-CompareLocal": {
            "commandName": "Project",
            "commandLineArgs": "compare-local -pf true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "RD"
            }
        },
        "GitHelperApp-CompareAzure": {
            "commandName": "Project",
            "commandLineArgs": "compare-azure -pf true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "RD"
            }
        },
        "GitHelperApp-CreatePr-DryRun": {
            "commandName": "Project",
            "commandLineArgs": "create-pr -pf true -d true -f true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "DR"
            }
        },
        "GitHelperApp-CreatePr-Azure-DryRun": {
            "commandName": "Project",
            "commandLineArgs": "create-pr -pf true -ct azure -d true -f true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "DR"
            }
        },
        "GitHelperApp-CreatePr": {
            "commandName": "Project",
            "commandLineArgs": "create-pr -pf true -f true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "DR"
            }
        },
        "GitHelperApp-CreatePr-Azure": {
            "commandName": "Project",
            "commandLineArgs": "create-pr -pf true -ct azure -f true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "MD"
            }
        },
        "GitHelperApp-SearchPr": {
            "commandName": "Project",
            "commandLineArgs": "search-pr -pf true -s active -c 5",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "RD"
            }
        },
        "GitHelperApp-SearchWorkItems": {
            "commandName": "Project",
            "commandLineArgs": "search-work-items -pf true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "MD"
            }
        },
        "GitHelperApp-SearchWorkItems-Azure": {
            "commandName": "Project",
            "commandLineArgs": "search-work-items -pf true -ct azure",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "MD"
            }
        },
        "GitHelperApp-CreateCustomPr": {
            "commandName": "Project",
            "commandLineArgs": "create-custom-pr",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "Local"
            }
        },
        "GitHelperApp-CreateCustomPr-DryRun": {
            "commandName": "Project",
            "commandLineArgs": "create-custom-pr -d true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "Local"
            }
        },
        "GitHelperApp-GetRepositories": {
            "commandName": "Project",
            "commandLineArgs": "get-repositories -pf true",
            "environmentVariables": {
                "GHA_ENVIRONMENT": "Local"
            }
        }
    }
}
```


### 1 Compare local repositories

The command able to run compare between branches (internally it used 'origin/*' to handle the even local repo but remote branches with actual
changes) for repositories. Configuration provided in the appsettings.json file for all repos and path to them cloned locally.
Here is using the [LibGit2Sharp](https://github.com/libgit2/libgit2sharp) library as official tool to with with Git.

*For this logic it needs to clone all the repositories locally to have actual configuration, folder with Git repo, remote (origin).*

Command arguments:

1. pc - IsPrintToConsole - is print results to console.
2. pf - IsPrintToFile - is print results to file.

Results:

1. File with compare result.

### 2 Compare on Azure DevOps

Process all the repositories from the configuration file and search for changes between branches. In he configuration we can provide the name
for bot branches to compare -source and destination.

Command arguments:

1. pc - IsPrintToConsole - is print results to console.
2. pf - IsPrintToFile - is print results to file.

Results:

1. File with compare result.

### 3 Create Pull Request

Here the logic based on searching he changes between branches locally and if they exists - create the Pull Requests on Azure. After this will be
generated full result with a lot of data.

*This command mostly used to prepare the PRs and identify changes for release - like Sprint 8 release and etc.*

The main entry point here to create the PRs - is compare result.

Command arguments:

1. pc - IsPrintToConsole - is print results to console.
2. pf - IsPrintToFile - is print results to file.
3. ct - CompareType - choose the tool to compare (local, azure).
4. f - IsFilter - do we need to filter work items?
5. d - DryRun - is dry run - without actual creation of the Pull Request.

Result:

1. Full results - the file with all results from processing (compare results, pull requests details, work items details).
2. Pull Request summary - summary of the Pull Requests summary.
3. Work Item summary - work items related to the Pull Requests.

### 4 Search Pull Request

Command to search the Pull Requests for repositories from configuration and provided the results with the list of PRs.

Command arguments:

1. pc - IsPrintToConsole - is print results to console.
2. pf - IsPrintToFile - is print results to file.
3. s - Status - status for PR to search. Possible values:
    - all
    - active
    - completed
    - abandoned
4. c - Count - how many records to fetch from API during search. (default = 10)

Result:

1. Pull requests results - list of the PR found and links to them grouped by repositories.

### 5 Search work items

Search for work items for the changes in the repositories. It allows to find the work items for git commits found as difference between branches.
This feature can be used to identify actual work items will be used for PRs as part of the release and verify that all the items added and not
added much more not needed by the links.

Command arguments:
1. pc - IsPrintToConsole - is print results to console.
2. pf - IsPrintToFile - is print results to file.
3. ct - CompareType - choose the tool to compare (local, azure).
4. f - IsFilter - is apply filter for work items or not.

Results:
1. Compare results and list of work items.

### 6 Create custom PR (single)

The special command to use to create the single PR based on some settings. Idea here was to use another user to create commit from his side.
But actually even if provide the another author it will use actual user who own the PAT token used to auth with Azure DevOps. So in this case it need to have additional configuration and functionality to support multi user support (not in scope and maybe in future only).

Command arguments:

1. d - DryRun - is dry run - without actual creation of the Pull Request.

Results:
1. Information about PR created.

### 7 Get repositories list

Search for all repositories inside team project and build the list of them.

Command arguments:

1. pc - IsPrintToConsole - is print results to console.
2. pf - IsPrintToFile - is print results to file.
3. tp - TeamProject - the name of the team project.

Results:

1. List of the repositories in specific team project (by name or from configuration by default).

## Application configuration

Application use configuration files - *appsettings.json*. For different situations - in our case - merging and comparing between branched. So basically here working as the Environment name - instead of names line Local, Development we are using the abbreviations for identify source and destination branch - like Dev to Release = DR. It can help to manage the settings and other configuration for specific branches.

There are additional files with settings settings:

1. appsettings.DR.json - configuration for Dev to Release.
2. appsettings.RD.json - configuration for Release to Dev.
3. appsettings.RM.json - configuration for Release to Main (master instead of main for some repositories).
4. appsettings.MD.json - configuration for Main to Dev (master instead of main for some repositories).
5. appsettings.RD.json - configuration for Release to Dev.

Main configuration placed in the appsettings.json file.

Configuration sections:

1. AppConfig - some of the application configuration line output path for results.
    - OutputDirectory - directory for saving the results after processing.
    - OutputFormat - format to use for output results (test, markdown) - used to create file in simple test or Markdown.
        - text - simple text file without any extra formatting.
        - markdown - Markdown format - extended text file with ability to render as HTML.
        - markdown-table - extended Markdown file with Tables to present the data in table format.
2. RepositoriesConfig - configuration for repositories, path for local repo and branches to use.
    - DefaultSourceBranch - the source branch to use for all repositories if not provided the branch for repository specific.
    - DefaultDestinationBranch - the destination branch to use for all repositories if not provided the branch for repository specific.
    - DefaultTeamProject - the project to use as Team Project.
    - Repositories - list of the repositories with settings.
        - Name - the name of repository - used for search in Azure DevOps and to print in results.
        - Path - local path to the folder with repository.
3. AzureDevOps - Azure DevOps settings (to use wot work with PR and compare on Azure).
    - Token - Personal Access Token with access to code and work items and PRs.
    - CollectionUrl - the Collection URL user for API client configuration.
    - TeamProject - the Main and default team project.
4. PullRequestConfig - configuration for pull requests.
    - Title - the title for new Pull Request.
    - Description - the description for new Pull Request.
    - IsDraft - create PR as draft.
    - Author - author name (same as in Azure DevOps). Names here need to use from Constants.
    - Tags - the list of tags to be added to the PR.
5. WorkItemFilterConfig - configuration to apply filters for Work Items in Azure DevOps
    - Types - item types (Feature, Bug, Story, Bug).
    - Areas - areas for items.
    - Iterations - array of iterations to use to filter.
    - WorkItemsToAdd - Work Items Ids - to be added to each PR will be created manually Sometimes it needed to add some WIT for PRs because no relations between comments.
6. CustomPrConfig - configuration for custom PR to be created as single one.
    - RepositoryName - name of the repository.
    - TeamProject - team project.
    - SourceBranch - source branch.
    - DestinationBranch - destination branch.
    - Author - author of he commit (not work as expected because overridden by actual user with token used to create the PR). So this is just test things that wanted to be working to able to create PR from another person.
    - Title - title for the custom PR.
    - Description - description.
    - IsDraft - is it will be draft PR.

All the additional configuration used internally in the code and if needed (depends on the use cases) can be moved to the config files in different
sections.

*Idea: Possible later user configuration can be moved to the config file to able to edit it in more simple way or will be used (need to find the
actual API to call the Azure DevOps to get such details).*

## Results

After processing the commands application may create the different files with the results. Here the list of the possible results (all of them as simple text files, as for now):

1. Compare results - some details about changes between branches in repositories.
2. Pull Request results - details about Pull Request and related Work Items for new PRs.
3. Work Items results - information about work items for PRs and summary from all the PRs with changes.

All results can be placed to the next files:

1. Prs-{guid}.txt (md) - pull request details.
2. Wit-{guid}.txt (md) - work items details.
3. ResultsFull-{guid}.txt (md) - full results.
4. Repositories-{giuid}.txt (md) - list of the repositories found in team project.

For each application run in the output folder generated every time. This folder has pattern for name:
**$"{commandName}-{DateTime.Now:dd-MM-yyyy-HH-mm}"** - command name and date and time. So it very basic solution.

Each file in the folder will have unique ID (GUID) generated after starting the tool in the same time when folder is created. Even if we may run more that one in same minute we will have one folder but 2 different set of files.

## Implementation notes

Application build as console tool with commands that can be parsed from the arguments, provide the help for them.

Main logic placed in the services:

1. AzureDevOpsService - service to handle all the logic to communicate with Azure DevOps via API (with client libraries).
2. CompareService - service to do compare process.
3. GitService - service to with with Git locally.
4. OutputService - service to handle all the logic to process results and output to console or file (processing data as lines of strings).
5. PullRequestService - logic to work with Pull Request like searching and create the new PRs.
6. WorkItemsService - logic to work with work items (specially to each and etc.).
7. BaseSharedService - some shared logic for services we can use (if needed to have the same methods or other code parts).
8. RepositoryService - logic to work with the repositories.

Operations in the application handled with the [CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) and special classes named as commands:

1. SearchPrCommand - command to do the search Pull requests.
2. CreatePrCommand - command to run compare and create PR for repositories with changes (only for them, if no changes available we
don't need to create PR).
3. CompareLocalCommand - command to run the local compare - need to have all the repositories cloned locally.
4. CompareAzureCommand - command to run compare repositories on Azure DevOps.
5. SearchWorkItemsCommand - command to run the search for Work Items.
6. GetRepositoriesCommand - command to get the list of repositories in team project.
7. CreateCustomPrCommand - command to create the custom single PR.

Application also have separate logic for generation content based on results to output to file and console, logic to generate the filenames and etc.:

1. TextFileContentGenerator - generator to create the strings with results in the text format.
2. MarkdownContentGenerator - generator to create the strings with results in the Markdown format (better to read and use in some
other places because can be rendered).
3. MarkdownTableContentGenerator - generator to create the string with content in the Markdown format but with support of tables in
some lines instead of the usual line.
4. FileNameGenerator - logic to create names for files to save the results.
5. ContentGeneratorFactory - simple factory to use proper content generator for processing the results.

Most of the code use logger to put the messages to the log (console and file, can be added a lot of different sinks because of Serilog is used). Also added some documentation comments as well.
Most of the possible settings that can be changes or need to be different for comparing (like branches, PR details, etc.) included in the configuration so it can be simply changes without rebuilding the application.

---

## The features for future

*Here can be added ideas that we can try to think and implement in the tool in future.*

There are some ideas and possible featured to implement in the tool:

1. Run pipelines - currently API in client library in preview and don't allow to do this - test feature branch created but not working as for now.
2. Improve the processing results - single file, different formats, etc. - it’s very high level idea.
3. Create the simple web application to sue the tool for all team members and able to do some actions with repositories.
    - Here can be created simple pages to compare branches and get the results.
    - Create the PRs and get results.
    - Some configuration page maybe - specific settings for users (maybe static in config files to don’t add additional DB for such service).

Implemented features:

1. Build the table in Markdown with the full result to use on Release notes page - already used during large releases.

---

Using the tool during the release
For usual release we have a lot of changes in the different repositories and it hard to track all the changes for them. So the solutions is the new
tool that able to run comparison between branches and identify the changes and build list of them.

The tool help to:

1. Identify the repositories with changes that needed to be released (planed).
2. Create the new PRs for manually review and merge the changes (as part of the release process).
3. Create file with complete result of processing - compare data, work items list, PRs, etc.

Usual release process contains next steps to do with the code:

1. Create PRs from dev to release branch - use tool.
2. Deploy to Staging - manual process.
3. Smoke testing on Staging - manual.
4. Deploy to Production - manual.
5. Create PRs from Release to Main branch - use tool.

By using the tool we can:

1. Identify how many repositories affected.
2. Create new PRs.
3. Create and update the page with release notes.

So as we can see based on this steps and functionality the tool can optimize and improve the routines for release process and use results for
Release notes page.

How to use:

1. Run the tool to compare the branches - use predefined run profiles in launchsettings.json - GitHelperApp-CompareLocal or GitHelperApp-CompareAzure.
    - Review the results and identify how many repositories are needed to be merged.
2. Create the Release Notes pages (if not available yet).
    - For quick create can be used template - MM/DD/YYYY Sprint XX Release Notes - this is custom template used for all release notes pages and updated as much as possible to include all the changes and improvements.
3. Run the tool to create PR - use predefined run profiles in launchsettings.json - GitHelperApp-CreatePr.
    - Review the results.
    - Use ResultFull-5e73c87609894dc49194dbb6643f68b6.md (sample file name, each file have unique GUID identifier) or another file for PRs list.
    - Open in text editor the file and copy all the content and create the page in Confluence under release section and insert the data from file - it will automatically parse the Markdown and format properly with styles.
    - Update release notes page - add the link to the results page and upload the files with results.

## Links

Articles:

1. https://markheath.net/post/automate-prs-azure-devops
2. https://www.codeproject.com/Articles/3941198/Serverless-DevOps-Little-Helper

Documentation links:

1. https://docs.microsoft.com/en-us/azure/devops/integrate/concepts/dotnet-client-libraries?view=azure-devops - library from Microsoft for
working with the TFS/Azure DevOps.
2. https://docs.microsoft.com/en-us/rest/api/azure/devops/git/?view=azure-devops-rest-6.0 - Git API reference docs.
3. https://docs.microsoft.com/en-us/rest/api/azure/devops/wit/?view=azure-devops-rest-6.0 - Work item tracking reference docs.