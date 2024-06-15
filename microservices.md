# Learn about Microservices and Monolithic systems
## Some details about microservices
Monolithic vs Microservice - What and Why
	1. ![image](https://github.com/bhanu00/system-design/assets/24689682/61eabde7-1549-4821-9126-8f974ea80e47)

	2. Pros
		a. At first - simple and no over-engineering
		b. Single codebase
		c. Resource efficient at small scale
	3. Cons
		a. Modularity is hard to enforce as app grows
		b. Scaling is challenge - For example - if you have more load on one set of APIs not others then you need to scale full system (EC2 instance) as all APIs are part of single monolithic app
		c. ![image](https://github.com/bhanu00/system-design/assets/24689682/90db66ad-94bf-47a4-a452-4eb499d2016a)

		d. All or nothing deployment
		e. Long release cycles
		f. Slow to react to customer demands
	4. Can we use API Gateway with monolithic - YES 
		a. ![image](https://github.com/bhanu00/system-design/assets/24689682/278d9f81-f613-4643-9fa0-e7be9f8c08a5)

	5. APIs in Microservice
		a. Easy scaling
		b. You can develop different microservices in different languages (Polyglot)
		c. ![image](https://github.com/bhanu00/system-design/assets/24689682/e500f8f4-6213-42f3-86d3-ca2d3ac37ee0)

	6. Characteristics of Microservices:
		a. Independent
			i. Scaling
			ii. Governance
			iii. Deployment
			iv. Testing
			v. Functionality
	7. For ex, each microservice can run on EC2 instance or could be part of different ASGs (Autoscaling Groups). You can use ELB or API Gateway to host these microservices. 
	8. Beyond EC2 instances you can use Serverless like AWS Lambda to run the services. Or you can use container based approach (ECS & EKS)

## [Database Proxy and how it works?](https://medium.com/@danielaaronw/database-proxies-and-why-you-should-use-them-e44166f3b47b) and [Amazon RDS Proxy](https://aws.amazon.com/rds/proxy/) or [Database proxy](https://programmingbrain.com/2022/11/what-is-database-proxy)
## [Death by thousand microservices](https://renegadeotter.com/2023/09/10/death-by-a-thousand-microservices.html?utm_source=tldrwebdev)
