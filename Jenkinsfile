pipeline {
    agent {
        dockerfile {
            filename 'Dockerfile.ci'
            label 'docker'
            args '-u root:root -v /var/run/docker.sock:/var/run/docker.sock'
        }
    }
    environment {
        NEXUS_URL = 'artifacts.home.lab'
        NEXUS_REPO = 'docker-images'
    }
    stages {
        stage('Bundle connector daemon') {
            when {
                expression {
                    return env.BRANCH_NAME in ['dev', 'master'] || env.TAG_NAME
                }
            }
            steps {
                script {
                    def repoUrl = scm.userRemoteConfigs[0].url.replace('https://','')
                    withCredentials([usernamePassword(
                        credentialsId: 'jenkins-gitlab-https',
                        usernameVariable: 'GIT_USER',
                        passwordVariable: 'GIT_TOKEN'
                    )]) {
                        sh """
                            git fetch --tags --force https://\$GIT_USER:\$GIT_TOKEN@${repoUrl}
                        """
                    }
                }
                script {
                    if (env.TAG_NAME) {
                        env.VERSION = env.TAG_NAME
                    } else {
                        BASE_VERSION = sh(script: "git describe --tags --abbrev=0 || echo '0.0.1-alpha'", returnStdout: true).trim()
                        def suffix = ""
                        if (env.BRANCH_NAME == "dev") {
                            suffix = "-dev"
                        } else if (env.BRANCH_NAME == "master") {
                            suffix = "-staging"
                        }
                        env.VERSION = "${BASE_VERSION}${suffix}-${BUILD_NUMBER}"
                    }

                    echo "Computed VERSION: ${env.VERSION}"
                }

                sh '''
                    IMAGE_NAME=$NEXUS_URL/$NEXUS_REPO/jenkins-connector-daemon
                    docker build --no-cache -t $IMAGE_NAME:$VERSION -f src/connector-daemon/Dockerfile src/connector-daemon
                '''
            }
        }
        stage('Publish connector daemon') {
            when {
                expression {
                    return env.BRANCH_NAME in ['dev', 'master'] || env.TAG_NAME
                }
            }
            steps {
                withCredentials([usernamePassword(
                    credentialsId: 'jenkins-nexus',
                    usernameVariable: 'DOCKER_USER',
                    passwordVariable: 'DOCKER_PASSWORD'
                )]) {
                    sh '''
                        IMAGE_NAME=$NEXUS_URL/$NEXUS_REPO/jenkins-connector-daemon

                        echo "Logging in to Nexus Docker registry..."
                        echo "$DOCKER_PASSWORD" | docker login $NEXUS_URL -u "$DOCKER_USER" --password-stdin

                        docker tag $IMAGE_NAME:$VERSION $IMAGE_NAME:latest
                        docker push $IMAGE_NAME:$VERSION
                        docker push $IMAGE_NAME:latest
                    '''
                }
            }
        }
        stage('Bundle connector dashboard') {
            when {
                expression {
                    return env.BRANCH_NAME in ['dev', 'master'] || env.TAG_NAME
                }
            }
            steps {
                script {
                    def repoUrl = scm.userRemoteConfigs[0].url.replace('https://','')
                    withCredentials([usernamePassword(
                        credentialsId: 'jenkins-gitlab-https',
                        usernameVariable: 'GIT_USER',
                        passwordVariable: 'GIT_TOKEN'
                    )]) {
                        sh """
                            git fetch --tags --force https://\$GIT_USER:\$GIT_TOKEN@${repoUrl}
                        """
                    }
                }
                script {
                    if (env.TAG_NAME) {
                        env.VERSION = env.TAG_NAME
                    } else {
                        BASE_VERSION = sh(script: "git describe --tags --abbrev=0 || echo '0.0.1-alpha'", returnStdout: true).trim()
                        def suffix = ""
                        if (env.BRANCH_NAME == "dev") {
                            suffix = "-dev"
                        } else if (env.BRANCH_NAME == "master") {
                            suffix = "-staging"
                        }
                        env.VERSION = "${BASE_VERSION}${suffix}-${BUILD_NUMBER}"
                    }

                    echo "Computed VERSION: ${env.VERSION}"
                }

                sh '''
                    IMAGE_NAME=$NEXUS_URL/$NEXUS_REPO/jenkins-connector-dashboard
                    docker build --no-cache -t $IMAGE_NAME:$VERSION -f src/dashboard/Dockerfile src/dashboard
                '''
            }
        }
        stage('Publish connector dashboard') {
            when {
                expression {
                    return env.BRANCH_NAME in ['dev', 'master'] || env.TAG_NAME
                }
            }
            steps {
                withCredentials([usernamePassword(
                    credentialsId: 'jenkins-nexus',
                    usernameVariable: 'DOCKER_USER',
                    passwordVariable: 'DOCKER_PASSWORD'
                )]) {
                    sh '''
                        IMAGE_NAME=$NEXUS_URL/$NEXUS_REPO/jenkins-connector-dashboard

                        echo "Logging in to Nexus Docker registry..."
                        echo "$DOCKER_PASSWORD" | docker login $NEXUS_URL -u "$DOCKER_USER" --password-stdin

                        docker tag $IMAGE_NAME:$VERSION $IMAGE_NAME:latest
                        docker push $IMAGE_NAME:$VERSION
                        docker push $IMAGE_NAME:latest
                    '''
                }
            }
        }
    }
}
