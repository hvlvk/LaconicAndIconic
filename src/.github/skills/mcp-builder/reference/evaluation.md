# MCP Server Evaluation Guide

## Overview

This document provides guidance on creating comprehensive evaluations for MCP servers. Evaluations test whether LLMs can effectively use your MCP server to answer realistic, complex questions using only the tools provided.

---

## Quick Reference

### Evaluation Requirements
- Create 10 human-readable questions
- Questions must be READ-ONLY, INDEPENDENT, NON-DESTRUCTIVE
- Each question requires multiple tool calls (potentially dozens)
- Answers must be single, verifiable values
- Answers must be STABLE (won't change over time)

### Output Format
```xml
<evaluation>
   <qa_pair>
      <question>Your question here</question>
      <answer>Single verifiable answer</answer>
   </qa_pair>
</evaluation>
```

---

## Purpose of Evaluations

The measure of quality of an MCP server is NOT how well or comprehensively the server implements tools, but how well these implementations enable LLMs with no other context and access ONLY to the MCP servers to answer realistic and difficult questions.

## Question Guidelines

### Core Requirements

1. **Questions MUST be independent** — each question should NOT depend on the answer to any other question
2. **Questions MUST require ONLY NON-DESTRUCTIVE AND IDEMPOTENT tool use**
3. **Questions must be REALISTIC, CLEAR, CONCISE, and COMPLEX** — requiring multiple tools or steps

### Complexity and Depth

4. **Questions must require deep exploration** — multi-hop questions requiring multiple sub-questions
5. **Questions may require extensive paging** through results
6. **Questions must require deep understanding** rather than surface-level knowledge
7. **Questions must not be solvable with straightforward keyword search**

### Tool Testing

8. **Questions should stress-test tool return values**
9. **Questions should MOSTLY reflect real human use cases**
10. **Questions may require dozens of tool calls**
11. **Include ambiguous questions** — but ensure there is STILL A SINGLE VERIFIABLE ANSWER

### Stability

12. **Questions must be designed so the answer DOES NOT CHANGE** — use historical data and closed concepts

## Answer Guidelines

### Verification
- Answers must be VERIFIABLE via direct string comparison
- Specify output format in the QUESTION (e.g., "Use YYYY/MM/DD.", "Respond True or False.")

### Readability
- Answers should generally prefer HUMAN-READABLE formats (names, datetime, file name, URL, yes/no)

### Stability
- Look at old content (conversations that have ended, projects that have launched)
- Create QUESTIONS based on "closed" concepts that will always return the same answer

### Diversity
- Answers should be in diverse modalities and formats
- Answers must NOT be complex structures (not lists, not objects)

## Evaluation Process

### Step 1: Documentation Inspection
Read the documentation of the target API to understand available endpoints and functionality.

### Step 2: Tool Inspection
List the tools available in the MCP server. Understand input/output schemas without calling the tools.

### Step 3: Developing Understanding
Iterate multiple times. Think about the kinds of tasks you want to create. Do NOT read the code of the MCP server implementation.

### Step 4: Read-Only Content Inspection
USE the MCP server tools with READ-ONLY operations to identify specific content for creating questions. Make INCREMENTAL, SMALL, AND TARGETED tool calls.

### Step 5: Task Generation
Create 10 human-readable questions following all guidelines above.

## Output Format

```xml
<evaluation>
   <qa_pair>
      <question>Find the project created in Q2 2024 with the highest number of completed tasks. What is the project name?</question>
      <answer>Website Redesign</answer>
   </qa_pair>
   <qa_pair>
      <question>Search for issues labeled as "bug" that were closed in March 2024. Which user closed the most issues? Provide their username.</question>
      <answer>sarah_dev</answer>
   </qa_pair>
</evaluation>
```

## Evaluation Examples

### Good Questions

**Multi-hop question requiring deep exploration:**
```xml
<qa_pair>
   <question>Find the repository that was archived in Q3 2023 and had previously been the most forked project in the organization. What was the primary programming language used in that repository?</question>
   <answer>Python</answer>
</qa_pair>
```

**Requires understanding context without keyword matching:**
```xml
<qa_pair>
   <question>Locate the initiative focused on improving customer onboarding that was completed in late 2023. The project lead created a retrospective document after completion. What was the lead's role title at that time?</question>
   <answer>Product Manager</answer>
</qa_pair>
```

### Poor Questions

**Answer changes over time:**
```xml
<qa_pair>
   <question>How many open issues are currently assigned to the engineering team?</question>
   <answer>47</answer>
</qa_pair>
```

**Ambiguous answer format (list):**
```xml
<qa_pair>
   <question>List all the repositories that have Python as their primary language.</question>
   <answer>repo1, repo2, repo3</answer>
</qa_pair>
```

---

# Running Evaluations

## Setup

1. **Install Dependencies**
   ```bash
   pip install anthropic mcp
   ```

2. **Set API Key**
   ```bash
   export ANTHROPIC_API_KEY=your_api_key_here
   ```

## Running Evaluations

**STDIO transport** (script launches server automatically):
```bash
python scripts/evaluation.py -t stdio -c python -a my_server.py evaluation.xml
```

**HTTP transport** (start server first):
```bash
python scripts/evaluation.py -t http -u https://example.com/mcp evaluation.xml
```

## Output

The evaluation script generates a detailed report including:
- **Summary Statistics**: Accuracy, average duration, tool calls per task
- **Per-Task Results**: Prompt, expected/actual response, correct/incorrect status
