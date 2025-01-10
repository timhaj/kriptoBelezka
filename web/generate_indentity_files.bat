@echo off
setlocal enabledelayedexpansion

rem List of Identity files
set files=^
 Account.Manage._Layout^
 Account.Manage._ManageNav^
 Account.Manage._StatusMessage^
 Account.Manage.ChangePassword^
 Account.Manage.DeletePersonalData^
 Account.Manage.Disable2fa^
 Account.Manage.DownloadPersonalData^
 Account.Manage.Email^
 Account.Manage.EnableAuthenticator^
 Account.Manage.ExternalLogins^
 Account.Manage.GenerateRecoveryCodes^
 Account.Manage.Index^
 Account.Manage.PersonalData^
 Account.Manage.ResetAuthenticator^
 Account.Manage.SetPassword^
 Account.Manage.ShowRecoveryCodes^
 Account.Manage.TwoFactorAuthentication

rem Loop through each file and run the command
for %%f in (%files%) do (
    echo Running command for %%f
    dotnet aspnet-codegenerator identity -dc BelezkaContext -fi %%f -f
)

endlocal
