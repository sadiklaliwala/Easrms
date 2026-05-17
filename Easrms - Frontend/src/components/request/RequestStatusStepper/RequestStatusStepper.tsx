import { Step, StepLabel, Stepper } from "@mui/material";

const STATUS_STEPS = [
  "Open",
  "Pending Approval",
  "Approved",
  "Assigned",
  "In Progress",
  "Resolved",
  "Closed",
];

interface RequestStatusStepperProps {
  currentStatus: string;
}

const RequestStatusStepper = ({ currentStatus }: RequestStatusStepperProps) => {
  if (currentStatus === "Rejected") {
    return (
      <Stepper activeStep={-1} alternativeLabel>
        <Step key="Rejected" completed={false}>
          <StepLabel error>Rejected</StepLabel>
        </Step>
      </Stepper>
    );
  }

  const activeStep = STATUS_STEPS.indexOf(currentStatus);

  return (
    <Stepper activeStep={activeStep} alternativeLabel>
      {STATUS_STEPS.map((step) => (
        <Step key={step}>
          <StepLabel>{step}</StepLabel>
        </Step>
      ))}
    </Stepper>
  );
};

export default RequestStatusStepper;