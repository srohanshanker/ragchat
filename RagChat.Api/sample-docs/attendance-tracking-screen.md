# Attendance Tracking Screen

## Purpose
Lets a Case Manager or Site Administrator record daily attendance for each Active
student in a Program, and review attendance history for reporting.

## Who Can Access It
"Case Manager" and "Site Administrator" roles have full edit access. "Enrollment
Coordinator" has read-only access, useful when confirming a student's Active status
before scheduling.

## Fields
- **Session Date** — defaults to today; can be backdated up to 5 business days by a
  Case Manager, or any date by a Site Administrator.
- **Roster** — auto-populated from students with Enrollment Status = Active in the
  selected Program for the Session Date.
- **Attendance Code** — Present, Absent-Excused, Absent-Unexcused, Late.
- **Minutes Attended** — required when Attendance Code is Present or Late; used
  downstream by the Billing Summary Screen to calculate billable units.
- **Notes** — optional free-text field, visible to Case Managers only.

## Flow
1. Case Manager selects a Program and Session Date; the roster loads automatically.
2. Each student defaults to "Absent-Unexcused" until marked otherwise — this is a
   deliberate design choice so missed check-ins are never silently recorded as Present.
3. Saving the screen locks the session for edits after 48 hours unless a Site
   Administrator reopens it.
4. Attendance records feed the nightly Billing Summary calculation; a session saved
   after the nightly batch runs is included the following night.

## Common Errors
- "Session already locked" — shown when attempting to edit a session more than 48
  hours old; only a Site Administrator can reopen it via the Session Unlock action.
- "Minutes attended exceeds session length" — validation error when Minutes Attended
  is greater than the Program's configured session duration.

## Related Screens
- Student Enrollment Screen
- Billing Summary Screen
- Session Unlock (administrator action)
