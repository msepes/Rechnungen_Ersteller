$mail = New-Object System.Net.Mail.MailMessage
$mail.From = New-Object System.Net.Mail.MailAddress("algasim@max--clean.de")
$mail.To.Add("mo@msepes.de")
$mail.Subject = "Test Email"
$mail.Body = "This is the email body."

# Create SmtpClient and configure
$smtpServer = New-Object System.Net.Mail.SmtpClient("smtp.variomedia.de", 465)
$smtpServer.EnableSsl = $true
$smtpServer.Credentials = New-Object System.Net.NetworkCredential("algasim@max--clean.de", "Mhamad90")

$smtpServer.Send($mail)
Write-Host "Email sent successfully!"
