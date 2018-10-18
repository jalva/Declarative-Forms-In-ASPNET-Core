import { pardotFormSubmission } from './pardotFormSubmission.js';

var formSubmissionFactory = {
    getSubmission: function (submissionType) {
        if (submissionType == 'pardotIframe') // this string needs to come from the SubmissionTypeEnum
            return pardotFormSubmission;
        // add other supported form submission handlers here
        return null;
    }
};

export { formSubmissionFactory };