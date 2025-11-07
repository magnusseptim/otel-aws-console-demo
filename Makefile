.PHONY: docker-build up down logs hit demo-compose collector-validate tail-collector

docker-build:
	docker build -t otel-aws-console-demo:local -f docker/Dockerfile .

up:
	docker compose up -d --build

down:
	docker compose down -v

logs:
	docker compose logs -f otel-collector

hit:
	@curl -s http://localhost:8080/ping && echo
	@curl -s http://localhost:8080/work && echo

demo-compose: up
	@sleep 2
	$(MAKE) hit
	@echo "---- last 80 lines from collector ----"
	@docker compose logs --no-log-prefix otel-collector | tail -n 80

collector-validate:
	docker compose exec -T otel-collector /otelcol-contrib validate --config=/etc/otelcol/config.yaml
