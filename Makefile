.ONESHELL:
SHELL := /bin/bash
APP=OtelAwsConsoleDemo


restore:
	dotnet restore src/$(APP)

build: restore
	dotnet build src/$(APP) -c Release

run:
	dotnet run --project src/$(APP) -c Release

demo:
	set -euo pipefail
	dotnet run --project src/$(APP) -c Release &
	APP_PID=$$!
	sleep 2
	curl -s http://localhost:8080/ping ; echo
	curl -s http://localhost:8080/work ; echo
	kill $$APP_PID
	wait $$APP_PID || true

clean:
	rm -rf src/$(APP)/bin src/$(APP)/obj