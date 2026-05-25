import { useEffect, useRef } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Box, Typography, CircularProgress } from "@mui/material";
import toast from "react-hot-toast";

import {
  useOauthLoginMutation,
  useLinkProviderMutation,
} from "../../../store/api/auth.endpoints";
import { setCredentials } from "../../../store/slices/authSlice";
import { useAppDispatch } from "../../../hooks/useAppSelector";
import { ROLES } from "../../../constants/role.constants";
import type { AuthProvider } from "../../../types/common.types";

const OAuthCallbackPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const hasRun = useRef(false);

  const [oauthLogin] = useOauthLoginMutation();
  const [linkProvider] = useLinkProviderMutation();

  useEffect(() => {
    // Prevent double execution in StrictMode
    if (hasRun.current) return;
    hasRun.current = true;

    const handleCallback = async () => {
      const code = searchParams.get("code");
      const stateFromUrl = searchParams.get("state");

      // Validate state for CSRF protection
      const storedState = sessionStorage.getItem("oauth_state");
      if (!stateFromUrl || stateFromUrl !== storedState) {
        toast.error("Invalid OAuth state. Please try again.");
        sessionStorage.removeItem("oauth_state");
        sessionStorage.removeItem("oauth_flow");
        navigate("/login", { replace: true });
        return;
      }
      sessionStorage.removeItem("oauth_state");

      // Read flow info
      const flowData = sessionStorage.getItem("oauth_flow");
      sessionStorage.removeItem("oauth_flow");

      if (!code || !flowData) {
        toast.error("OAuth callback failed. Missing parameters.");
        navigate("/login", { replace: true });
        return;
      }

      let flow: { flow: "login" | "link"; provider: AuthProvider };
      try {
        flow = JSON.parse(flowData);
      } catch {
        toast.error("Invalid OAuth flow data.");
        navigate("/login", { replace: true });
        return;
      }

      if (flow.flow === "login") {
        // ── Login Flow ──
        try {
          const response = await oauthLogin({
            code,
            provider: flow.provider,
          }).unwrap();

          if (response.success && response.data) {
            dispatch(setCredentials(response.data));
            toast.success(`Welcome back, ${response.data.fullName}!`);

            // Redirect by role
            const role = response.data.roleName;
            if (role === ROLES.SUPPORT_USER) {
              navigate("/my-tasks", { replace: true });
            } else {
              navigate("/dashboard", { replace: true });
            }
          } else {
            toast.error(response.message ?? "OAuth login failed");
            navigate("/login", { replace: true });
          }
        } catch (err: any) {
          const message =
            err?.data?.message ??
            err?.data?.errors?.[0] ??
            "OAuth login failed. Please try again.";
          toast.error(message);
          navigate("/login", { replace: true });
        }
      } else {
        // ── Link Flow ──
        try {
          const response = await linkProvider({
            code,
            provider: flow.provider,
          }).unwrap();

          if (response.success) {
            toast.success("Provider linked successfully!");
          } else {
            toast.error(response.message ?? "Failed to link provider");
          }
        } catch (err: any) {
          const message =
            err?.data?.message ??
            err?.data?.errors?.[0] ??
            "Failed to link provider";
          toast.error(message);
        }
        navigate("/profile", { replace: true });
      }
    };

    handleCallback();
  }, [searchParams, navigate, dispatch, oauthLogin, linkProvider]);

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        gap: 2,
        bgcolor: "background.default",
      }}
    >
      <CircularProgress size={48} />
      <Typography variant="body1" sx={{ color: "text.secondary", fontWeight: 500 }}>
        Processing authentication...
      </Typography>
    </Box>
  );
};

export default OAuthCallbackPage;
