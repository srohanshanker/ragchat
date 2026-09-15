# Billing Summary Screen

## Purpose
Gives a Billing Specialist a month-end view of billable units generated from
attendance records, grouped by Program and funding source, before submission to the
payer.

## Who Can Access It
"Billing Specialist" and "Site Administrator" roles only. Case Managers cannot access
this screen, since it may surface funding-source details considered sensitive.

## Fields
- **Billing Period** — month/year selector; defaults to the most recently closed
  period.
- **Program** — filter; "All Programs" is the default view.
- **Funding Source** — filter; a student's funding source is set on their Enrollment
  record and copied onto each attendance-derived billing line.
- **Billable Units** — computed as Minutes Attended divided by the funding source's
  configured unit length, rounded down.
- **Status** — Draft, Submitted, Paid, Rejected.

## Flow
1. The nightly batch job aggregates the prior day's locked attendance sessions into
   billing lines; the Billing Summary Screen only ever displays already-aggregated
   data, it does not compute live from raw attendance.
2. A Billing Specialist reviews the Draft lines for a period, flags any that look
   wrong, and either edits the underlying attendance session (which re-triggers
   aggregation) or annotates the line with a note.
3. Once reviewed, the specialist clicks **Submit Period**, which locks all lines in
   that period to Submitted and generates the payer export file.
4. Payer responses are recorded manually as Paid or Rejected; a Rejected line requires
   a resubmission note before it can be re-submitted in a later batch.

## Common Errors
- "Cannot submit — unresolved sessions" — shown when a Program in the selected period
  still has attendance sessions in "unlocked" state; all sessions must be locked before
  submission.
- "Funding source missing on student" — a billing line with no funding source is
  excluded from the export and flagged in red; it usually means the student's
  Enrollment record was never fully approved.

## Related Screens
- Attendance Tracking Screen
- Student Enrollment Screen
- Payer Export History
