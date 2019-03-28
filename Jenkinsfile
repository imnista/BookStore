#!groovy​

/*
    Jenkins plugin dependency list
        - Basic plugins
        - AnsiColor
        - MSTest
        - Email Extension Plugin
        - Workspace Cleanup
        - Azure App Service
        - Azure CLI
*/

// # Relay on [MSTest] plugin
import hudson.tasks.test.AbstractTestResultAction

// Define the global variable
def UnitTestResult = "NOT EXECUTED"
def CurrentVersion = ""
def LatestGitCommitMessage = ""
def LatestGitCommitter = ""
def BuildResultFolder = ""
def BaseVersion = "0.1.0"

pipeline {

    agent any

    environment {
        // Debug options
        IS_DEBUGING = "true" // #

        // Credentials
        JENKINS_AZURE_CREDENTIALS_ID = "cp-dev-rg_bclsdev-cp-api-jenkins" // #

        // Azure Resources
        AZURE_SUBSCRIPTION = "bcls-ngsprep-dev" // #
        AZURE_RESOURCE_GROUP = "dl-bookstore-rg" // #
        AZURE_APP_NAME = "bookstore-web" // #

        // Building, testing, and publishing configurations
        BUILD_CONFIG = "Release" // #
        PUBLISH_SOURCE_DIRECTORY = "src/BeckmanCoulter.BookStore/bin/Release/netcoreapp2.1/publish/"
        CP_WEBAPI_HOST_FOLDER = "src/BeckmanCoulter.BookStore"
        UNIT_TEST_RESULT_FOLDER = "src/UnitTestTarget.Tests/TestResults"
        UNIT_TEST_LOG_FILE_NAME = "unit_tests.xml"

        NUGET_SERVERS = "http://nuget.org/api/v2;https://api.nuget.org/v3/index.json;"

        // Common information
        PROJECT_NAME = "Beckman Book Store project - Dev Env" // #
        EMAIL_GROUP_DEBUG = "hqi@beckman.com"
        EMAIL_GROUP_EVERYONE = "hqi@beckman.com SZHONG@beckman.com LFU01@beckman.com HHAO01@beckman.com QSUN@beckman.com"
    }

    triggers {
        // Configuration of Additional Behaviours: includedRegions/excludedMessage in Jenkinsfile is not supported yet.
        // https://stackoverflow.com/questions/37294781/configuration-of-includedregions-in-jenkinsfile

        // Check GitHub submit request
        pollSCM("* * * * *") // Every minute

        // Build every day 1:00am UTC+8
        // cron("0 12 * * *")
    }

    options {
        timeout(time: 1, unit: "HOURS")
    }

    stages {

        stage('Information Retrieval') {
            steps {
                show "Start to retrieve information - ${PROJECT_NAME} ..."

                // Retrieval the site URL of the deploy target
                azureCLI commands: [
                    [script: 'az account set -s ' + AZURE_SUBSCRIPTION],
                    [exportVariablesString: '/defaultHostName|PUBLISHED_SITE_URL', script: 'az webapp show --resource-group ' + AZURE_RESOURCE_GROUP + ' --name ' + AZURE_APP_NAME],
                ], principalCredentialId: JENKINS_AZURE_CREDENTIALS_ID
                script {
                    PUBLISHED_SITE_URL = "https://${PUBLISHED_SITE_URL}"
                    show PUBLISHED_SITE_URL
                }

                // Get the Build Version and create the Build Result folder
                script {
                    CurrentVersion = "${BaseVersion.trim()}.${env.BUILD_NUMBER}"
                    show "Current Version: ${CurrentVersion}"
                }
            }
        }

        stage('Checkout') {
             steps {
                show "Start to checkout code - ${PROJECT_NAME} ..."

                // # Relay on [Workspace Cleanup] plugin
                cleanWs()
                checkout scm
                script {
                    LatestGitCommitMessage = sh (
                        script: "git show -s --format=%B",
                        returnStdout: true
                    ).trim()

                    LatestGitCommitter = sh (
                        script: "git --no-pager show -s --format=%ae",
                        returnStdout: true
                    ).trim()
                }
            }
        }

        stage('Package Prepare') {
            steps {
                show "Start to restore packages - ${PROJECT_NAME} ..."
                sh "dotnet restore src -s \'${NUGET_SERVERS}\'"
            }
        }

        stage('Build') {
            steps {
                show "Start to build - ${PROJECT_NAME} ..."
                sh "dotnet build src --configuration ${BUILD_CONFIG} --no-restore -p:Version=${CurrentVersion}"
                sh "dotnet publish src --configuration ${BUILD_CONFIG} --no-restore --no-build"
            }
        }

        stage('Unit Test') {
            steps {
                show "Start to test - ${PROJECT_NAME} ..."
                sh returnStatus: true, script: "dotnet test src --configuration ${BUILD_CONFIG} --logger \"trx;LogFileName=${UNIT_TEST_LOG_FILE_NAME}\" --no-build"

                step([$class: 'MSTestPublisher', testResultsFile:"**/${UNIT_TEST_LOG_FILE_NAME}", failOnError: true, keepLongStdio: true])

                script {
                    def (isAllTestPass, testStatus, total, failed, skipped, passed) = AnalyzeUnitTest()
                    UnitTestResult = testStatus

                    if (isAllTestPass == false)
                    {
                        show "Unit test failed - ${PROJECT_NAME}", 'r'
                        show "Test status: ${testStatus}", 'r'

                        SendEmail(
                            EMAIL_GROUP_EVERYONE,
                            "[Jenkins] ${env.JOB_NAME} #${env.BUILD_NUMBER} test FAILED!",
                            "Hi team, \n\n"
                            + "${PROJECT_NAME} test failed!\n\n"
                            + "Build Time: ${new Date()} \n\n"
                            + "Build URL: ${env.BUILD_URL}\n"
                            + "Repository: ${GIT_URL}\n"
                            + "Branch: ${GIT_BRANCH}\n"
                            + "--------------------------------------\n"
                            + "Latest Commit: ${LatestGitCommitMessage} (${GIT_COMMIT})\n"
                            + "Committer: ${LatestGitCommitter}\n"
                            + "--------------------------------------\n"
                            + "Unit Test Result: ${UnitTestResult}\n\n"

                            + "Regards,\n.NET Learning Group")

                        assert false
                    }
                    else
                    {
                        show "Unit test run success - ${PROJECT_NAME}", 'g'
                        show "Test status: ${testStatus}", 'g'
                    }
                }
            }
        }


        stage('Deploy') {
            steps {
                show "Start to deploy - ${PROJECT_NAME} ..."
                show "JENKINS_AZURE_CREDENTIALS_ID: ${JENKINS_AZURE_CREDENTIALS_ID}"
                show "Start to publish project to Azure Web App: ${AZURE_APP_NAME}"
                azureWebAppPublish(
                    azureCredentialsId: JENKINS_AZURE_CREDENTIALS_ID,
                    resourceGroup: AZURE_RESOURCE_GROUP,
                    appName: AZURE_APP_NAME,
                    sourceDirectory: PUBLISH_SOURCE_DIRECTORY,
                    deployOnlyIfSuccessful: true
                    )
            }
        }

        stage('Deploy Verify') {
            steps {
                show "Start to verify the deploy - ${PROJECT_NAME} ..."
                show "Visiting the URL: ${PUBLISHED_SITE_URL}"
                HttpGetRequest(PUBLISHED_SITE_URL)
            }
        }
    }

    post {

        always {
            show 'Jenkins task finish', 'b'
        }

        failure {
            show 'Jenkins task failed', 'r'
            SendEmail(
                EMAIL_GROUP_EVERYONE,
                "[Jenkins] ${env.JOB_NAME} #${env.BUILD_NUMBER} build FAILED!",
                "Hi team, \n\n"

                + "${PROJECT_NAME} build failed!\n\n"
                + "Build Time: ${new Date()} \n\n"
                + "Build URL: ${env.BUILD_URL}\n"
                + "Repository: ${GIT_URL}\n"
                + "Branch: ${GIT_BRANCH}\n"
                + "--------------------------------------\n"
                + "Latest Commit: ${LatestGitCommitMessage} (${GIT_COMMIT})\n"
                + "Committer: ${LatestGitCommitter}\n"
                + "--------------------------------------\n"
                + "Unit Test Result: ${UnitTestResult}\n\n"

                + "Regards,\n.NET Learning Group")
        }

        success {
            show 'Jenkins task success', 'g'
            SendEmail(
                EMAIL_GROUP_EVERYONE,
                "[Jenkins] ${env.JOB_NAME} #${env.BUILD_NUMBER} build and deploy successfully",
                "Hi team, \n\n"

                + "${PROJECT_NAME} build and deploy successfully.\n\n"
                + "Build Time: ${new Date()} \n\n"
                + "Build URL: ${env.BUILD_URL}\n"
                + "Repository: ${GIT_URL}\n"
                + "Branch: ${GIT_BRANCH}\n"
                + "--------------------------------------\n"
                + "Latest Commit: ${LatestGitCommitMessage} (${GIT_COMMIT})\n"
                + "Committer: ${LatestGitCommitter}\n"
                + "--------------------------------------\n"
                + "Unit Test Result: ${UnitTestResult}\n\n"

                + "Host URL: ${PUBLISHED_SITE_URL}\n\n"

                + "Regards,\n.NET Learning Group")
        }

        unstable {
             show 'Build has gone unstable', 'y'
        }

    }

}

// https://stackoverflow.com/questions/41171550/jenkins-java-io-notserializableexception-groovy-util-slurpersupport-nodechild
@NonCPS
def GetCurrentVersionByCpHostProjectFile() {
    // Read host project version in the file (XML) and JSON
    def projectConfig = readFile "${env.WORKSPACE}/${CP_WEBAPI_HOST_FOLDER}/Cp.WebApi.Host.csproj"
    def rootNode = new XmlSlurper(false, false).parseText(projectConfig)
    def baseVersionNode = rootNode.PropertyGroup[0].Version[0];

    def baseVersion = baseVersionNode.text();
    show "Base Version: ${baseVersion}"
    sh "echo ${baseVersion} > version.txt"
}

@NonCPS
def GetFileValue() {
    def content = readFile "version"
    return content.trim()
}

def GetShellValue(value) {
    return sh (
        script: "echo ${value}",
        returnStdout: true
    ).trim().toString()
}

def GenerateFile(content, filepath) {
    sh "echo \'${content}\' > ${filepath}"
}

@NonCPS
def HttpGetRequest(url) {
    // Need to approved following method signatures
    // org.jenkinsci.plugins.scriptsecurity.scripts.ScriptApproval.get().approveSignature("method java.net.URL openConnection")
    // org.jenkinsci.plugins.scriptsecurity.scripts.ScriptApproval.get().approveSignature("method java.net.HttpURLConnection getResponseCode")
    // org.jenkinsci.plugins.scriptsecurity.scripts.ScriptApproval.get().approveSignature("method java.net.URLConnection getInputStream")
    // org.jenkinsci.plugins.scriptsecurity.scripts.ScriptApproval.get().approveSignature("staticMethod org.codehaus.groovy.runtime.DefaultGroovyMethods getText java.io.InputStream")

    // GET
    def get = new URL(url).openConnection()
    def responseHttpCode = get.getResponseCode()
    show "Http response code ${responseHttpCode}"
    if(responseHttpCode.equals(200)) {
        // show get.getInputStream().getText()
        show "Successful access the site, return - 200 OK"
    }
}

def SendEmail(mailRecipients, subject, body) {
    show "Sending email to: ${IS_DEBUGING == "false" ? mailRecipients : EMAIL_GROUP_DEBUG}", 'y'
    mail (
        to: IS_DEBUGING == "false"  ? mailRecipients : EMAIL_GROUP_DEBUG,
        subject: subject,
        body: body
    )

    // https://wiki.jenkins-ci.org/display/JENKINS/Email-ext+plugin
    // # Relay on [Email Extension Plugin] plugin
    // emailext (
    //     to: "hqi@beckman.com SZHONG@beckman.com LFU01@beckman.com HHAO01@beckman.com QSUN@beckman.com",
    //     subject: "[TEST] ${env.JOB_NAME} #${env.BUILD_NUMBER} [${currentBuild.result}]",
    //     body: "Hi team, \n\n Build, test and deploy completed. Build URL: ${env.BUILD_URL}.",
    //     attachLog: true,
    // )
}


def show(String text, String style = "i", Boolean isHightlight = true) {

    // https://blog.johnwu.cc/article/jenkins-ansi-color-console-output.html

    ansiColor("xterm") {

        switch (style.charAt(0)) {
            // Info - blue
            case "i":
                echo "\033[34m ${text} \033[0m"
                break;
            // Red
            case "r":
                if (isHightlight) echo "\033[41m ${text} \033[0m"
                else echo "\033[31m ${text} \033[0m"
                break;
            // Green
            case "g":
                if (isHightlight) echo "\033[42m ${text} \033[0m"
                else echo "\033[32m ${text} \033[0m"
                break;
            // Yellow
            case "y":
                if (isHightlight) echo "\033[43m ${text} \033[0m"
                else echo "\033[33m ${text} \033[0m"
                break;
            // Blue
            case "b":
                if (isHightlight) echo "\033[44m ${text} \033[0m"
                else echo "\033[34m ${text} \033[0m"
                break;
        }

    }
}

def AnalyzeUnitTest() {
    // It's all relay on the MSTest plugin and neet import hudson.tasks.test.AbstractTestResultAction
    // And need to approved following method signatures
    // method hudson.model.Actionable getAction java.lang.Class
    // method hudson.tasks.test.AbstractTestResultAction getFailCount
    // method hudson.tasks.test.AbstractTestResultAction getFailureDiffString
    // method hudson.tasks.test.AbstractTestResultAction getSkipCount
    // method hudson.tasks.test.AbstractTestResultAction getTotalCount
    // method org.jenkinsci.plugins.workflow.support.steps.build.RunWrapper getRawBuild

    def testStatus = ""
    def allTestPass = 0
    def total = 0
    def failed = 0
    def skipped = 0
    def passed = 0

    AbstractTestResultAction testResultAction = currentBuild.rawBuild.getAction(AbstractTestResultAction.class)
    if (testResultAction != null) {
        total = testResultAction.totalCount
        failed = testResultAction.failCount
        skipped = testResultAction.skipCount
        passed = total - failed - skipped
        // testStatus = "Passed: ${passed}, Failed: ${failed} ${testResultAction.failureDiffString}, Skipped: ${skipped}"
        testStatus = "Passed: ${passed}, Failed: ${failed}, Skipped: ${skipped}"
        // failCount will return Gets the total number of failed tests.
        // whereas failureDiffString returns Gets the diff string of failures
        if (failed == 0) {
            currentBuild.result = 'SUCCESS'
        }
        allTestPass = (failed == 0)
    }
    else
    {
        testStatus = "Did not find any tests..."
        allTestPass = false
    }
    println testStatus

    return [allTestPass, testStatus, total, failed, skipped, passed]
}
