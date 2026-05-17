import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Stack, Grid } from '@mui/material';
import toast from 'react-hot-toast';

import {
  useGetRequestByIdQuery,
  useApproveOrRejectRequestMutation,
  useAssignRequestMutation,
  useUpdateRequestStatusMutation,
  useCloseRequestMutation,
} from '../../../store/api/request.endpoints';
import { useGetCommentsQuery, useAddCommentMutation } from '../../../store/api/comment.endpoints';
import { useGetStatusHistoryQuery } from '../../../store/api/comment.endpoints';
import { useGetSupportUsersQuery } from '../../../store/api/lookup.endpoints';
import { useAppSelector } from '../../../hooks/useAppSelector';

import AppPageHeader from '../../../components/common/layout/AppPageHeader';
import AppCard from '../../../components/common/layout/AppCard';
import AppBreadcrumb from '../../../components/common/layout/AppBreadcrumb';
import AppLoader from '../../../components/common/feedback/AppLoader';
import AppErrorState from '../../../components/common/feedback/AppErrorState';

import RequestDetailHeader from '../../../components/request/RequestDetailHeader';
import RequestMetaInfo from '../../../components/request/RequestMetaInfo';
import RequestRejectionBanner from '../../../components/request/RequestRejectionBanner';
import RequestStatusStepper from '../../../components/request/RequestStatusStepper';
import RequestHistoryTimeline from '../../../components/request/RequestHistoryTimeline';
import RequestCommentBox from '../../../components/request/RequestCommentBox';
import RequestActionButtons from '../../../components/request/RequestActionButtons';

import ApprovalDialog from '../../../components/common/modal/ApprovalDialog';
import AssignSupportDialog from '../../../components/common/modal/AssignSupportDialog';
import UpdateStatusDialog from '../../../components/common/modal/UpdateStatusDialog';
import CloseRequestDialog from '../../../components/common/modal/CloseRequestDialog';

import type { ApprovalRequestDto } from '../../../types/request.types';
import type { AddCommentDto } from '../../../types/comment.types';

// ─── Component ────────────────────────────────────────────────────────────────
const RequestDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { userId, roleName } = useAppSelector((state) => state.auth);

  // ─── Dialog State ─────────────────────────────────────────────────────────────
  const [approvalOpen, setApprovalOpen] = useState(false);
  const [assignOpen, setAssignOpen] = useState(false);
  const [statusOpen, setStatusOpen] = useState(false);
  const [closeOpen, setCloseOpen] = useState(false);

  // ─── Queries ──────────────────────────────────────────────────────────────────
  const { data: requestResponse, isLoading, isError } = useGetRequestByIdQuery(id!);
  const { data: commentsResponse } = useGetCommentsQuery(id!);
  const { data: historyResponse } = useGetStatusHistoryQuery(id!);
  const { data: supportUsersResponse } = useGetSupportUsersQuery();

  // ─── Mutations ────────────────────────────────────────────────────────────────
  const [approveOrReject, { isLoading: approving }] = useApproveOrRejectRequestMutation();
  const [assignRequest, { isLoading: assigning }] = useAssignRequestMutation();
  const [updateStatus, { isLoading: updatingStatus }] = useUpdateRequestStatusMutation();
  const [closeRequest, { isLoading: closing }] = useCloseRequestMutation();
  const [addComment, { isLoading: commenting }] = useAddCommentMutation();

  if (isLoading) return <AppLoader />;
  if (isError || !requestResponse?.success) return <AppErrorState message="Failed to load request details" />;

  const request = requestResponse.data;
  const comments = commentsResponse?.data ?? [];
  const history = historyResponse?.data ?? [];
  const supportUsers = supportUsersResponse?.data ?? [];

  // ─── Handlers ─────────────────────────────────────────────────────────────────
  const handleApproval = async (data: ApprovalRequestDto) => {
    try {
      const res = await approveOrReject({ id: id!, body: data }).unwrap();
      if (res.success) {
        toast.success(`Request ${data.action === 'Approve' ? 'approved' : 'rejected'} successfully`);
        setApprovalOpen(false);
      } else {
        toast.error(res.message ?? 'Action failed');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Action failed');
    }
  };

  const handleAssign = async (supportUserId: string) => {
    try {
      const res = await assignRequest({ id: id!, body: { supportUserId } }).unwrap();
      if (res.success) {
        toast.success('Request assigned successfully');
        setAssignOpen(false);
      } else {
        toast.error(res.message ?? 'Assignment failed');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Assignment failed');
    }
  };

  const handleStatusUpdate = async (newStatus: string, remarks: string) => {
    try {
      const res = await updateStatus({ id: id!, body: { newStatus, remarks } }).unwrap();
      if (res.success) {
        toast.success('Status updated successfully');
        setStatusOpen(false);
      } else {
        toast.error(res.message ?? 'Status update failed');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Status update failed');
    }
  };

  const handleClose = async () => {
    try {
      const res = await closeRequest(id!).unwrap();
      if (res.success) {
        toast.success('Request closed successfully');
        setCloseOpen(false);
        navigate('/requests');
      } else {
        toast.error(res.message ?? 'Failed to close request');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Failed to close request');
    }
  };

  const handleAddComment = async (data: AddCommentDto) => {
    try {
      const res = await addComment({ requestId: id!, body: data }).unwrap();
      if (res.success) {
        toast.success('Comment added');
      } else {
        toast.error(res.message ?? 'Failed to add comment');
      }
    } catch (err: any) {
      toast.error(err?.data?.message ?? 'Failed to add comment');
    }
  };

  return (
    <Stack spacing={3}>
      {/* Breadcrumb */}
      <AppBreadcrumb
        items={[
          { label: 'Requests', path: '/requests' },
          { label: request.requestNumber },
        ]}
      />

      <AppPageHeader
        title="Request Details"
        actions={
          <RequestActionButtons
            request={request}
            onApprove={() => setApprovalOpen(true)}
            onAssign={() => setAssignOpen(true)}
            onUpdateStatus={() => setStatusOpen(true)}
            onClose={() => setCloseOpen(true)}
          />
        }
      />

      {/* Rejection Banner */}
      {request.rejectionReason && (
        <RequestRejectionBanner reason={request.rejectionReason} />
      )}

      {/* Status Stepper */}
      <AppCard>
        <RequestStatusStepper currentStatus={request.status} />
      </AppCard>

      <Grid container spacing={3}>
        {/* Left — Request Info */}
        <Grid item xs={12} md={8}>
          <Stack spacing={3}>
            <AppCard>
              <RequestDetailHeader request={request} />
            </AppCard>

            <AppCard>
              <RequestMetaInfo request={request} />
            </AppCard>

            {/* Comments */}
            <AppCard>
              <RequestCommentBox
                comments={comments}
                onAddComment={handleAddComment}
                isSubmitting={commenting}
              />
            </AppCard>
          </Stack>
        </Grid>

        {/* Right — History */}
        <Grid item xs={12} md={4}>
          <AppCard>
            <RequestHistoryTimeline history={history} />
          </AppCard>
        </Grid>
      </Grid>

      {/* Dialogs */}
      <ApprovalDialog
        open={approvalOpen}
        onClose={() => setApprovalOpen(false)}
        onApprove={(comment) => handleApproval({ action: 'Approve', comment })}
        onReject={(comment) => handleApproval({ action: 'Reject', comment })}
        loading={approving}
      />

      <AssignSupportDialog
        open={assignOpen}
        onClose={() => setAssignOpen(false)}
        supportUsers={supportUsers}
        onAssign={handleAssign}
        loading={assigning}
      />

      <UpdateStatusDialog
        open={statusOpen}
        onClose={() => setStatusOpen(false)}
        currentStatus={request.status}
        onUpdate={handleStatusUpdate}
        loading={updatingStatus}
      />

      <CloseRequestDialog
        open={closeOpen}
        onClose={() => setCloseOpen(false)}
        onConfirm={handleClose}
        loading={closing}
      />
    </Stack>
  );
};

export default RequestDetailPage;