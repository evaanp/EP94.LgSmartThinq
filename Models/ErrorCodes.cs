using System;
using System.Collections.Generic;
using System.Text;

namespace EP94.LgSmartThinq.Models
{
    public static class ErrorCodes
    {
        public static int OK = 0000;
        public static int PARTIAL_OK = 0001;
        public static int OPERATION_IN_PROGRESS_DEVICE = 0103;
        public static int PORTAL_INTERWORKING_ERROR = 0007;
        public static int PROCESSING_REFRIGERATOR = 0104;
        public static int RESPONSE_DELAY_DEVICE = 0111;
        public static int SERVICE_SERVER_ERROR = 8107;
        public static int SSP_ERROR = 8102;
        public static int TIME_OUT = 9020;
        public static int WRONG_XML_OR_URI = 9000;

        public static int AWS_IOT_ERROR = 8104;
        public static int AWS_S3_ERROR = 8105;
        public static int AWS_SQS_ERROR = 8106;
        public static int BASE64_DECODING_ERROR = 9002;
        public static int BASE64_ENCODING_ERROR = 9001;
        public static int CLIP_ERROR = 8103;
        public static int CONTROL_ERROR_REFRIGERATOR = 0105;
        public static int CREATE_SESSION_FAIL = 9003;
        public static int DB_PROCESSING_FAIL = 9004;
        public static int DM_ERROR = 8101;
        public static int DUPLICATED_ALIAS = 0013;
        public static int DUPLICATED_DATA = 0008;
        public static int DUPLICATED_LOGIN = 0004;
        public static int EMP_AUTHENTICATION_FAILED = 0102;
        public static int ETC_COMMUNICATION_ERROR = 8900;
        public static int ETC_ERROR = 9999;
        public static int EXCEEDING_LIMIT = 0112;
        public static int EXPIRED_CUSTOMER_NUMBER = 0119;
        public static int EXPIRES_SESSION_BY_WITHDRAWAL = 9005;
        public static int FAIL = 0100;
        public static int INACTIVE_API = 8001;
        public static int INSUFFICIENT_STORAGE_SPACE = 0107;
        public static int INVAILD_CSR = 9010;
        public static int INVALID_BODY = 0002;
        public static int INVALID_CUSTOMER_NUMBER = 0118;
        public static int INVALID_HEADER = 0003;
        public static int INVALID_PUSH_TOKEN = 0301;
        public static int INVALID_REQUEST_DATA_FOR_DIAGNOSIS = 0116;
        public static int MISMATCH_DEVICE_GROUP = 0014;
        public static int MISMATCH_LOGIN_SESSION = 0114;
        public static int MISMATCH_NONCE = 0006;
        public static int MISMATCH_REGISTRED_DEVICE = 0115;
        public static int MISSING_SERVER_SETTING_INFORMATION = 9005;
        public static int NOT_AGREED_TERMS = 0110;
        public static int NOT_CONNECTED_DEVICE = 0106;
        public static int NOT_CONTRACT_CUSTOMER_NUMBER = 0120;
        public static int NOT_EXIST_DATA = 0010;
        public static int NOT_EXIST_DEVICE = 0009;
        public static int NOT_EXIST_MODEL_JSON = 0117;
        public static int NOT_REGISTERED_SMART_CARE = 0121;
        public static int NOT_SUPPORTED_COMMAND = 0012;
        public static int NOT_SUPPORTED_COUNTRY = 8000;
        public static int NOT_SUPPORTED_SERVICE = 0005;
        public static int NO_INFORMATION_DR = 0109;
        public static int NO_INFORMATION_SLEEP_MODE = 0108;
        public static int NO_PERMISSION = 0011;
        public static int NO_PERMMISION_MODIFY_RECIPE = 0113;
        public static int NO_REGISTERED_DEVICE = 0101;
        public static int NO_USER_INFORMATION = 9006;
    }
}
