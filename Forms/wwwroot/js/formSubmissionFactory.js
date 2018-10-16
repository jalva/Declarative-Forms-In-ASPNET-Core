import { pardotFormSubmission } from './pardotFormSubmission.js';

var formSubmissionFactory = {
    getSubmission: function (submissionType) {
        if (submissionType == 'pardot-iframe')
            return pardotFormSubmission;
        // add other supported form submission handlers here
        return null;
    }
};

export { formSubmissionFactory };