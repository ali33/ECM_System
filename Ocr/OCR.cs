/**
 * Copyright @ 2008 Quan Nguyen
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.ComponentModel;

namespace Ecm.Ocr
{
    public abstract class OCR<T>
    {
        protected Rectangle rect = Rectangle.Empty;
        BackgroundWorker worker;
        public const string PSM_OSD_ONLY = "0";
        public const string PSM_AUTO_OSD = "1";
        public const string PSM_AUTO_ONLY = "2";
        public const string PSM_AUTO = "3";
        public const string PSM_SINGLE_COLUMN = "4";
        public const string PSM_SINGLE_BLOCK_VERT_TEXT = "5";
        public const string PSM_SINGLE_BLOCK = "6";
        public const string PSM_SINGLE_LINE = "7";
        public const string PSM_SINGLE_WORD = "8";
        public const string PSM_CIRCLE_WORD = "9";
        public const string PSM_SINGLE_CHAR = "10";
        public const string PSM_COUNT = "11";

        private string psm = PSM_AUTO;

        public string PSM
        {
            get { return psm; }
            set { psm = value; }
        }

        public string RecognizeText(IList<T> imageEntities, string lang, Rectangle selection)
        {
            rect = selection;
            return RecognizeText(imageEntities, lang);
        }
        /// <summary>
        /// Recognize text
        /// </summary>
        /// <param name="imageEntities"></param>
        /// <param name="index"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public abstract string RecognizeText(IList<T> imageEntities, string lang);

        public string RecognizeText(IList<T> imageEntities, string lang, Rectangle selection, BackgroundWorker worker, DoWorkEventArgs e)
        {
            rect = selection;
            return RecognizeText(imageEntities, lang, worker, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageEntities">list of imageEntities</param>
        /// <param name="index">index of page (frame) of image; -1 for all</param>
        /// <param name="lang">the language OCR is going to be performed for</param>
        /// <returns>result text</returns>
        public string RecognizeText(IList<T> imageEntities, string lang, BackgroundWorker worker, DoWorkEventArgs e)
        {
            // Abort the operation if the user has canceled.
            // Note that a call to CancelAsync may have set 
            // CancellationPending to true just after the
            // last invocation of this method exits, so this 
            // code will not have the opportunity to set the 
            // DoWorkEventArgs.Cancel flag to true. This means
            // that RunWorkerCompletedEventArgs.Cancelled will
            // not be set to true in your RunWorkerCompleted
            // event handler. This is a race condition.
            this.worker = worker;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return String.Empty;
            }

            return RecognizeText(imageEntities, lang);
        }

        void ProgressEvent(int percent)
        {
            worker.ReportProgress(percent);
        }
    }
}
