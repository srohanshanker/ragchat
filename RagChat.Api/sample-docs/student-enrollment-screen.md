# Student Enrollment Screen

## Purpose
The Student Enrollment screen lets front-desk staff register a new student into the
Student Services Portal, or update an existing student's enrollment record for a new
term. It is the entry point for every other workflow in the system — a student cannot
be scheduled, billed, or tracked for attendance until an enrollment record exists.

## Who Can Access It
Available to users with the "Enrollment Coordinator" or "Site Administrator" role.
Read-only access is granted to "Case Manager" roles so they can verify enrollment status
without editing it.

## Fields
- **Student ID** — auto-generated on first save; read-only afterward.
- **Full Name** — required, split into First/Middle/Last.
- **Date of Birth** — required; must result in an age between 3 and 21 at the start of
  the selected term, otherwise the screen shows a validation banner.
- **Guardian Contact** — at least one phone number or email is required.
- **Program** — dropdown sourced from the active Program list for the site; disabled
  once attendance records exist for the student in the current term.
- **Enrollment Status** — Active, Pending, Withdrawn, or Graduated.

## Flow
1. Coordinator searches for the student by name or ID. If no match is found, the
   screen offers a "New Enrollment" action.
2. On save, the system validates required fields and date-of-birth eligibility.
3. A successful save transitions the record to **Pending** until a supervisor approves
   it from the Enrollment Approvals queue.
4. Once approved, status becomes **Active** and the student becomes visible in
   Attendance Tracking and Scheduling.

## Common Errors
- "Duplicate student detected" — shown when name + date of birth matches an existing
  record; the coordinator is prompted to open the existing record instead of creating
  a duplicate.
- "Program is at capacity" — shown when the selected Program has reached its configured
  seat limit for the term; an override requires Site Administrator approval.

## Related Screens
- Enrollment Approvals (supervisor queue)
- Attendance Tracking Screen
- Billing Summary Screen
