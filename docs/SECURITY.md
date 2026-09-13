# Security

Rounder for Windows runs as a normal user process and does not require administrator privileges.

## Design

- Corner and glow effects are ordinary click-through layered windows.
- Settings are local JSON files under the current user profile.
- Launch-at-login uses the current-user Run registry key only.
- The app does not expose a local server or listen on network ports.
- The single-instance activation channel is local to the current Windows session.

## Release signing

Current public builds are not Authenticode-signed. Windows SmartScreen may therefore warn on first launch. Verify releases against this repository and its GitHub Actions workflow.

## Reporting issues

Please report security-relevant problems privately to the project owner when possible. Avoid posting secrets, tokens, or private machine information in public issues.
