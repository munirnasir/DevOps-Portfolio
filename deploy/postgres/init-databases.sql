-- Creates a dedicated database per microservice (database-per-service pattern).
-- Runs once, on first initialization of the Postgres data volume.
CREATE DATABASE catalogdb;
CREATE DATABASE salesdb;
