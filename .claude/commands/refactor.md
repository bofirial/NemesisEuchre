---
description: Refactor code with specific patterns
arguments:
  - name: target
    description: Refactor target
    required: true
    argument-hint: [target] (e.g., ClassName, project_name, or file_path)
---

**Analyze current implementation** - Understand existing code:
   - Search for target: "{target}" across codebase
   - Identify all usages and dependencies
   - Understand current patterns and structure
   - Document existing behavior and interfaces

**Refactor {target} using clean-code principles**:
- Extract methods for clarity
- Improve variable naming
- Reduce complexity
- Apply SOLID principles
- Add appropriate comments
- Consolidate duplicate code
- Fix Coupling and cohesion issues
- Fix Performance bottlenecks
- Address Maintainability concerns
- Create tests to cover Testing Gaps

**Assess refactoring impact** - Evaluate scope and risk:
   - Files and components affected
   - Breaking change potential
   - Test coverage requirements
   - Performance implications
   - Migration complexity
   - Rollback considerations

**Refactoring guidelines:**
- Maintain existing behavior (no functional changes in Phase 1)
- Ensure comprehensive test coverage

```bash
# Run tests after refactoring
dotnet test
```