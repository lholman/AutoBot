# Helper script for those who want to run
# psake without importing the module.

$scriptPath = Split-Path $MyInvocation.InvocationName
import-module (join-path $scriptPath psake.psm1)
invoke-psake @args
remove-module psake