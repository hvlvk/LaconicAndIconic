# MCP Server Best Practices# MCP Server Best Practices

























































































































































































































































- Document rate limits and performance characteristics- Specify required permissions and access levels- Document security considerations- Include working examples (at least 3 per major feature)- Provide clear documentation of all tools and capabilities## Documentation Requirements---- **Error handling**: Ensure proper error reporting and cleanup- **Performance testing**: Check behavior under load, timeouts- **Security testing**: Validate auth, input sanitization, rate limiting- **Integration testing**: Test interaction with external systems- **Functional testing**: Verify correct execution with valid/invalid inputsComprehensive testing should cover:## Testing Requirements---```}  };    }]      text: `Error: ${error.message}. Try using filter='active_only' to reduce results.`      type: "text",    content: [{    isError: true,  return {} catch (error) {  return { content: [{ type: "text", text: result }] };  const result = performOperation();try {```typescriptExample error handling:- Clean up resources properly on errors- Don't expose internal implementation details- Provide helpful, specific error messages with suggested next steps- Report tool errors within result objects (not protocol-level errors)- Use standard JSON-RPC error codes## Error Handling---**Important**: Annotations are hints, not security guarantees. Clients should not make security-critical decisions based solely on annotations.| `openWorldHint` | boolean | true | Tool interacts with external entities || `idempotentHint` | boolean | false | Repeated calls with same args have no additional effect || `destructiveHint` | boolean | true | Tool may perform destructive updates || `readOnlyHint` | boolean | false | Tool does not modify its environment ||-----------|------|---------|-------------|| Annotation | Type | Default | Description |Provide annotations to help clients understand tool behavior:## Tool Annotations---- Bind to `127.0.0.1` rather than `0.0.0.0`- Validate the `Origin` header on all incoming connections- Enable DNS rebinding protectionFor streamable HTTP servers running locally:### DNS Rebinding Protection- Clean up resources after errors- Provide helpful but not revealing error messages- Log security-relevant errors server-side- Don't expose internal errors to clients### Error Handling- Use schema validation (Pydantic/Zod) for all inputs- Prevent command injection in system calls- Check parameter sizes and ranges- Validate URLs and external identifiers- Sanitize file paths to prevent directory traversal### Input Validation- Provide clear error messages when authentication fails- Validate keys on server startup- Store API keys in environment variables, never in code**API Keys**:- Only accept tokens specifically intended for your server- Validate access tokens before processing requests- Use secure OAuth 2.1 with certificates from recognized authorities**OAuth 2.1**:### Authentication and Authorization## Security Best Practices---| **Real-time** | No | Yes || **Complexity** | Low | Medium || **Clients** | Single | Multiple || **Deployment** | Local | Remote ||-----------|-------|-----------------|| Criterion | stdio | Streamable HTTP |### Transport Selection**Note**: stdio servers should NOT log to stdout (use stderr for logging)- Single-user, single-session scenarios- Integrating with desktop applications- Building tools for local development environments**Use when**:- Runs as a subprocess of the client- Simple setup, no network configuration needed- Standard input/output stream communication**Characteristics**:**Best for**: Local integrations, command-line tools### stdio- Integration with web applications- Deploying as a cloud service- Serving multiple clients simultaneously**Use when**:- Enables server-to-client notifications- Can be deployed as a web service- Supports multiple simultaneous clients- Bidirectional communication over HTTP**Characteristics**:**Best for**: Remote servers, web services, multi-client scenarios### Streamable HTTP## Transport Options---```}  "next_offset": 20  "has_more": true,  "items": [...],  "offset": 0,  "count": 20,  "total": 150,{```jsonExample pagination response:- **Default to reasonable limits**: 20-50 items is typical- **Never load all results into memory**: Especially important for large datasets- **Return pagination metadata**: Include `has_more`, `next_offset`/`next_cursor`, `total_count`- **Implement pagination**: Use `offset` or cursor-based pagination- **Always respect the `limit` parameter**For tools that list resources:## Pagination---- Omit verbose metadata- Show display names with IDs in parentheses- Convert timestamps to human-readable format- Use headers, lists, and formatting for clarity- Human-readable formatted text### Markdown Format (`response_format="markdown"`, typically default)- Use for programmatic processing- Consistent field names and types- Include all available fields and metadata- Machine-readable structured data### JSON Format (`response_format="json"`)All tools that return data should support multiple formats:## Response Formats---- Keep tool operations focused and atomic- Provide tool annotations (readOnlyHint, destructiveHint, idempotentHint, openWorldHint)- Descriptions must precisely match actual functionality- Tool descriptions must narrowly and unambiguously describe functionality### Tool Design4. **Be specific**: Avoid generic names that could conflict with other servers3. **Be action-oriented**: Start with verbs (get, list, search, create, etc.)   - Use `github_create_issue` instead of just `create_issue`   - Use `slack_send_message` instead of just `send_message`2. **Include service prefix**: Anticipate that your MCP server may be used alongside other MCP servers1. **Use snake_case**: `search_users`, `create_project`, `get_channel_info`### Tool Naming## Tool Naming and Design---The name should be general, descriptive of the service being integrated, easy to infer from the task description, and without version numbers.- Examples: `slack-mcp-server`, `github-mcp-server`, `jira-mcp-server`**Node/TypeScript**: Use format `{service}-mcp-server` (lowercase with hyphens)- Examples: `slack_mcp`, `github_mcp`, `jira_mcp`**Python**: Use format `{service}_mcp` (lowercase with underscores)Follow these standardized naming patterns:## Server Naming Conventions---- Avoid SSE (deprecated in favor of streamable HTTP)- **stdio**: For local integrations, command-line tools- **Streamable HTTP**: For remote servers, multi-client scenarios### Transport- Default to 20-50 items- Return `has_more`, `next_offset`, `total_count`- Always respect `limit` parameter### Pagination- Markdown for human readability- JSON for programmatic processing- Support both JSON and Markdown formats### Response Formats- Example: `slack_send_message`, `github_create_issue`- Format: `{service}_{action}_{resource}`- Use snake_case with service prefix### Tool Naming- **Node/TypeScript**: `{service}-mcp-server` (e.g., `slack-mcp-server`)- **Python**: `{service}_mcp` (e.g., `slack_mcp`)### Server Naming## Quick Reference
## Quick Reference

### Server Naming
- **Python**: `{service}_mcp` (e.g., `slack_mcp`)
- **Node/TypeScript**: `{service}-mcp-server` (e.g., `slack-mcp-server`)

### Tool Naming
- Use snake_case with service prefix
- Format: `{service}_{action}_{resource}`
- Example: `slack_send_message`, `github_create_issue`

### Response Formats
- Support both JSON and Markdown formats
- JSON for programmatic processing
- Markdown for human readability

### Pagination
- Always respect `limit` parameter
- Return `has_more`, `next_offset`, `total_count`
- Default to 20-50 items

### Transport
- **Streamable HTTP**: For remote servers, multi-client scenarios
- **stdio**: For local integrations, command-line tools
- Avoid SSE (deprecated in favor of streamable HTTP)

---

## Server Naming Conventions

Follow these standardized naming patterns:

**Python**: Use format `{service}_mcp` (lowercase with underscores)
- Examples: `slack_mcp`, `github_mcp`, `jira_mcp`

**Node/TypeScript**: Use format `{service}-mcp-server` (lowercase with hyphens)
- Examples: `slack-mcp-server`, `github-mcp-server`, `jira-mcp-server`

The name should be general, descriptive of the service being integrated, easy to infer from the task description, and without version numbers.

---

## Tool Naming and Design

### Tool Naming

1. **Use snake_case**: `search_users`, `create_project`, `get_channel_info`
2. **Include service prefix**: Anticipate that your MCP server may be used alongside other MCP servers
   - Use `slack_send_message` instead of just `send_message`
   - Use `github_create_issue` instead of just `create_issue`
3. **Be action-oriented**: Start with verbs (get, list, search, create, etc.)
4. **Be specific**: Avoid generic names that could conflict with other servers

### Tool Design

- Tool descriptions must narrowly and unambiguously describe functionality
- Descriptions must precisely match actual functionality
- Provide tool annotations (readOnlyHint, destructiveHint, idempotentHint, openWorldHint)
- Keep tool operations focused and atomic

---

## Response Formats

All tools that return data should support multiple formats:

### JSON Format (`response_format="json"`)
- Machine-readable structured data
- Include all available fields and metadata
- Consistent field names and types
- Use for programmatic processing

### Markdown Format (`response_format="markdown"`, typically default)
- Human-readable formatted text
- Use headers, lists, and formatting for clarity
- Convert timestamps to human-readable format
- Show display names with IDs in parentheses
- Omit verbose metadata

---

## Pagination

For tools that list resources:

- **Always respect the `limit` parameter**
- **Implement pagination**: Use `offset` or cursor-based pagination
- **Return pagination metadata**: Include `has_more`, `next_offset`/`next_cursor`, `total_count`
- **Never load all results into memory**: Especially important for large datasets
- **Default to reasonable limits**: 20-50 items is typical

Example pagination response:
```json
{
  "total": 150,
  "count": 20,
  "offset": 0,
  "items": [...],
  "has_more": true,
  "next_offset": 20
}
```

---

## Transport Options

### Streamable HTTP

**Best for**: Remote servers, web services, multi-client scenarios

**Characteristics**:
- Bidirectional communication over HTTP
- Supports multiple simultaneous clients
- Can be deployed as a web service
- Enables server-to-client notifications

**Use when**:
- Serving multiple clients simultaneously
- Deploying as a cloud service
- Integration with web applications

### stdio

**Best for**: Local integrations, command-line tools

**Characteristics**:
- Standard input/output stream communication
- Simple setup, no network configuration needed
- Runs as a subprocess of the client

**Use when**:
- Building tools for local development environments
- Integrating with desktop applications
- Single-user, single-session scenarios

**Note**: stdio servers should NOT log to stdout (use stderr for logging)

### Transport Selection

| Criterion | stdio | Streamable HTTP |
|-----------|-------|-----------------|
| **Deployment** | Local | Remote |
| **Clients** | Single | Multiple |
| **Complexity** | Low | Medium |
| **Real-time** | No | Yes |

---

## Security Best Practices

### Authentication and Authorization

**OAuth 2.1**:
- Use secure OAuth 2.1 with certificates from recognized authorities
- Validate access tokens before processing requests
- Only accept tokens specifically intended for your server

**API Keys**:
- Store API keys in environment variables, never in code
- Validate keys on server startup
- Provide clear error messages when authentication fails

### Input Validation

- Sanitize file paths to prevent directory traversal
- Validate URLs and external identifiers
- Check parameter sizes and ranges
- Prevent command injection in system calls
- Use schema validation (Pydantic/Zod) for all inputs

### Error Handling

- Don't expose internal errors to clients
- Log security-relevant errors server-side
- Provide helpful but not revealing error messages
- Clean up resources after errors

### DNS Rebinding Protection

For streamable HTTP servers running locally:
- Enable DNS rebinding protection
- Validate the `Origin` header on all incoming connections
- Bind to `127.0.0.1` rather than `0.0.0.0`

---

## Tool Annotations

Provide annotations to help clients understand tool behavior:

| Annotation | Type | Default | Description |
|-----------|------|---------|-------------|
| `readOnlyHint` | boolean | false | Tool does not modify its environment |
| `destructiveHint` | boolean | true | Tool may perform destructive updates |
| `idempotentHint` | boolean | false | Repeated calls with same args have no additional effect |
| `openWorldHint` | boolean | true | Tool interacts with external entities |

**Important**: Annotations are hints, not security guarantees. Clients should not make security-critical decisions based solely on annotations.

---

## Error Handling

- Use standard JSON-RPC error codes
- Report tool errors within result objects (not protocol-level errors)
- Provide helpful, specific error messages with suggested next steps
- Don't expose internal implementation details
- Clean up resources properly on errors

Example error handling:
```typescript
try {
  const result = performOperation();
  return { content: [{ type: "text", text: result }] };
} catch (error) {
  return {
    isError: true,
    content: [{
      type: "text",
      text: `Error: ${error.message}. Try using filter='active_only' to reduce results.`
    }]
  };
}
```

---

## Testing Requirements

Comprehensive testing should cover:

- **Functional testing**: Verify correct execution with valid/invalid inputs
- **Integration testing**: Test interaction with external systems
- **Security testing**: Validate auth, input sanitization, rate limiting
- **Performance testing**: Check behavior under load, timeouts
- **Error handling**: Ensure proper error reporting and cleanup

---

## Documentation Requirements

- Provide clear documentation of all tools and capabilities
- Include working examples (at least 3 per major feature)
- Document security considerations
- Specify required permissions and access levels
- Document rate limits and performance characteristics
