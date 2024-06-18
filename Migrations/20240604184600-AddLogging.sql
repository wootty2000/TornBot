CREATE TABLE Logs (
      Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
      Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
      Level VARCHAR(50) NOT NULL,  -- Log level (e.g., Error, Info, Warning)
      Message TEXT NOT NULL,        -- Log message content
      Logger VARCHAR(255) DEFAULT NULL,  -- Optional: Name of the logger that generated the message
      Exception MEDIUMTEXT DEFAULT NULL  -- Optional: Serialized exception details (if applicable)
);