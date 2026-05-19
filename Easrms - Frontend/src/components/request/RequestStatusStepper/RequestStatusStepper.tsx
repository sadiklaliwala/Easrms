import { Step, StepLabel, Stepper } from "@mui/material";
import { STATUS, type StatusType } from "../../../constants/status.constants";

const STATUS_STEPS = [
  STATUS.OPEN,
  STATUS.PENDING_APPROVAL,
  STATUS.APPROVED,
  STATUS.ASSIGNED,
  STATUS.IN_PROGRESS,
  STATUS.RESOLVED,
  STATUS.CLOSED,
];

interface RequestStatusStepperProps {
  currentStatus: StatusType;
}

const RequestStatusStepper = ({ currentStatus }: RequestStatusStepperProps) => {
  if (currentStatus === STATUS.REJECTED) {
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
