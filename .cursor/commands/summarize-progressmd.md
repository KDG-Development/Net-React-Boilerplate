In the context of the work that has been completed:

- Format as a list of hyphenated line items, 2-5 lines per section with sensible reasoning for lines count vs time/effort
- Each section should have a non-hyphenated headline summarizing the group
- Break into sections of similar activities
- Provide each section as a plaintext ```code snippet``` for easy copy+paste
- Maximum 2 hours per section, rounded to nearest 6th minute (6 min increments)
- Time formatted as Hours:Minutes (e.g., 2:00, 1:42, 0:30) provided on its own line above its corresponding group
- Sections less than 2:00 do not need to be split up
- Don't assume! Prompt the user for total time spent in order to accurately break up timelog entries
- Don't make anything up

Section format:
```
[Headline] - [Time]
- [Detail item]
- [Detail item]
```

Language guidelines:
- Write for non-technical stakeholders who don't know or care how software works
- Describe WHAT was accomplished, not HOW it was built
- Use simple action verbs: "set up", "built", "added", "enabled", "connected", "fixed"
- Avoid technical jargon: no "framework", "architecture", "integration", "infrastructure", "refactor"
- No file names, code references, or technical paths
- Focus on business outcomes and user-facing results

Language examples:
- Bad: "Architected data synchronization framework for NetSuite integration"
- Good: "Set up automatic product sync between NetSuite and online store"
- Bad: "Refactored authentication infrastructure with OAuth2 implementation"
- Good: "Improved login security by adding OAuth2"

Output example:
```
Product Sync Setup - 2:00
- Set up automatic daily sync to pull product data from NetSuite
- Added ability to push product updates to online store
- Built system to handle large product catalogs efficiently
```
