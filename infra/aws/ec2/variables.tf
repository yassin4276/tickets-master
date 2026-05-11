variable "project_name" {
  type        = string
  description = "Project name used for resource naming"
  default     = "ticketing"
}

variable "aws_region" {
  type        = string
  description = "AWS region"
  default     = "eu-central-1"
}

variable "availability_zone" {
  type        = string
  description = "Availability zone for the public subnet"
  default     = "eu-central-1a"
}

variable "vpc_cidr" {
  type        = string
  description = "CIDR block for the VPC"
  default     = "10.10.0.0/16"
}

variable "public_subnet_cidr" {
  type        = string
  description = "CIDR block for the public subnet"
  default     = "10.10.1.0/24"
}

variable "instance_type" {
  type        = string
  description = "EC2 instance type"
  default     = "t3.micro"
}

variable "root_volume_size" {
  type        = number
  description = "Root EBS volume size in GB"
  default     = 20
}

variable "public_key_path" {
  type        = string
  description = "Path to SSH public key"
  default     = "~/.ssh/ticketing-aws.pub"
}

variable "my_ip_cidr" {
  type        = string
  description = "Your public IP in CIDR format, example: 1.2.3.4/32"
}