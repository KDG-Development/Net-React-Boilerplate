const semver = require('semver')
const { engines } = require('./package.json')

const version = engines.node

if (!semver.satisfies(process.version, version)) {
  console.error(
    '\x1b[31m%s\x1b[0m',
    `Required node version ${version} not satisfied with current version ${process.version}.

    Please update your Node.js installation to continue.
    Visit https://nodejs.org/ to download the latest version.
  `)
 
  process.exit(1)
}