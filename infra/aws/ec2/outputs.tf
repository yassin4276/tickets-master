output "instance_public_ip" {
  description = "Public IP of the EC2 instance"
  value       = aws_instance.app.public_ip
}

output "instance_public_dns" {
  description = "Public DNS of the EC2 instance"
  value       = aws_instance.app.public_dns
}

output "ssh_command" {
  description = "SSH command to connect to the EC2 instance"
  value       = "ssh -i ~/.ssh/ticketing-aws ubuntu@${aws_instance.app.public_ip}"
}

output "app_url" {
  description = "Temporary app URL on port 8080"
  value       = "http://${aws_instance.app.public_ip}:8080"
}