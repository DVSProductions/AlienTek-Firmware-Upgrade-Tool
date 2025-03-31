using AlienTek_Firmware_Upgrade_Tool;

namespace T12Test.NewUpdateFw;

internal class GetOrSetParameter(HIDManager manageHid) {
	public async Task<(ApplicationStatus status, DevInfo? devInfo)> GetDevInfo() {
		try {
			if(await manageHid.SendCMDWaitResult(16, 0, []) == ApplicationStatus.STATUS_OK) {
				return Utils.BytesToStuct(manageHid.RecEntity.Data, typeof(DevInfo)) is DevInfo result
					? ((ApplicationStatus status, DevInfo? devInfo))(ApplicationStatus.STATUS_OK, result)
					: ((ApplicationStatus status, DevInfo? devInfo))(ApplicationStatus.ERROR_UNKNOWN, null);
			}
		}
		catch {
		}
		return (ApplicationStatus.ERROR_UNKNOWN, null);
	}

	public async Task<ApplicationStatus> SetFwInfo(FirmInfo fileTop) {
		return await manageHid.SendCMDWaitResult(17, 0, Utils.StructToBytes(fileTop)) == ApplicationStatus.STATUS_OK && manageHid.RecEntity.Data[0] == 1
			? ApplicationStatus.STATUS_OK
			: ApplicationStatus.ERROR_UNKNOWN;
	}

	public async Task<ApplicationStatus> SetStartSendFW() {
		return await manageHid.SendCMDWaitResult(18, 0, []) == ApplicationStatus.STATUS_OK && manageHid.RecEntity.Data[0] == 1
			? ApplicationStatus.STATUS_OK
			: ApplicationStatus.ERROR_UNKNOWN;
	}

	public async Task<ApplicationStatus> SetSendData(byte seq, byte[] data) {
		return await manageHid.SendCMDWaitResult(19, seq, data) == ApplicationStatus.STATUS_OK && manageHid.RecEntity.Data[0] == 1
			? ApplicationStatus.STATUS_OK
			: ApplicationStatus.ERROR_UNKNOWN;
	}

	public async Task<ApplicationStatus> SetSendDataEnd() {
		return await manageHid.SendCMDWaitResult(20, 0, []) == ApplicationStatus.STATUS_OK && manageHid.RecEntity.Data[0] == 1
			? ApplicationStatus.STATUS_OK
			: ApplicationStatus.ERROR_UNKNOWN;
	}

	public async Task<ApplicationStatus> SetBoot() {
		await manageHid.SendCMDWaitResult(21, 0, []);
		return ApplicationStatus.STATUS_OK;
	}
}
